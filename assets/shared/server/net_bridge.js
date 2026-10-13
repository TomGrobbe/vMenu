// Linux TLS bridge: C# on Linux cannot validate root certificates, so it sends HTTPS and WebSocket traffic here.
// Local events reach every resource, so event names carry a Diffie-Hellman agreed secret.
const crypto = require('crypto');
const http = require('http');
const https = require('https');

const self = GetCurrentResourceName();
const sockets = new Map();
let prefix = null;

const fromSelf = () => GetInvokingResource() === self;

on('vMenu.Enhanced:NetBridge:Hello', (theirs) => {
    if (!fromSelf() || prefix !== null) {
        return;
    }

    try {
        const dh = crypto.getDiffieHellman('modp14');
        dh.generateKeys();

        const secret = dh.computeSecret(theirs, 'hex').toString('hex').replace(/^0+/, '');
        const hash = crypto.createHash('sha256').update(secret).digest('hex').slice(0, 32);

        prefix = `vMenu.Enhanced:NetBridge:${hash}:`;
        listen();

        const mine = dh.getPublicKey('hex');
        setImmediate(() => emit('vMenu.Enhanced:NetBridge:HelloBack', mine));
    } catch (error) {
        console.error(`[vMenu] Net bridge handshake failed: ${error?.message}`);
    }
});

on('onResourceStop', (resource) => {
    if (resource !== self) {
        return;
    }

    for (const ws of sockets.values()) {
        try {
            ws.close();
        } catch {
        }
    }
});

// Plain http(s) rather than fetch: fetch throws an unhandled "Reader released" rejection after some responses.
function send(request, url, signal, redirects) {
    return new Promise((resolve, reject) => {
        const target = new URL(url);
        const headers = { ...request.headers };
        if (request.body != null) {
            headers['Content-Length'] = Buffer.byteLength(request.body);
        }

        const client = target.protocol === 'http:' ? http : https;
        const req = client.request(target, { method: request.method, headers, signal }, (res) => {
            if (res.statusCode >= 300 && res.statusCode < 400 && res.headers.location && request.method === 'GET' && redirects > 0) {
                res.resume();
                try {
                    send(request, new URL(res.headers.location, target).toString(), signal, redirects - 1).then(resolve, reject);
                } catch (error) {
                    reject(error);
                }

                return;
            }

            const chunks = [];
            res.on('data', (chunk) => chunks.push(chunk));
            res.on('end', () => resolve({
                status: res.statusCode,
                body: Buffer.concat(chunks).toString('utf8'),
                retryAfter: res.headers['retry-after'] ?? null,
            }));
            res.on('error', reject);
        });

        req.on('error', reject);
        req.end(request.body ?? undefined);
    });
}

function listen() {
    on(prefix + 'Http', async (id, json) => {
        if (!fromSelf()) {
            return;
        }

        let reply;
        try {
            const request = JSON.parse(json);
            reply = await send(request, request.url, AbortSignal.timeout(request.timeoutMs), 5);
        } catch (error) {
            reply = error?.name === 'AbortError' || error?.name === 'TimeoutError'
                ? { timedOut: true }
                : { error: `${error?.name}: ${error?.message}${error?.cause ? ` (${error.cause.message ?? error.cause})` : ''}` };
        }

        emit(prefix + 'HttpDone', id, JSON.stringify(reply));
    });

    on(prefix + 'WsOpen', (id, url, headersJson) => {
        if (!fromSelf()) {
            return;
        }

        let ws;
        try {
            ws = new WebSocket(url, { headers: JSON.parse(headersJson) });
        } catch (error) {
            emit(prefix + 'WsClosed', id, `${error?.name}: ${error?.message}`);
            return;
        }

        sockets.set(id, ws);

        ws.onopen = () => emit(prefix + 'WsOpened', id);
        ws.onmessage = (event) => {
            if (typeof event.data === 'string') {
                emit(prefix + 'WsMessage', id, event.data);
            }
        };
        ws.onclose = (event) => {
            sockets.delete(id);
            emit(prefix + 'WsClosed', id, `${event.code} ${event.reason}`);
        };
    });

    on(prefix + 'WsSend', (id, text) => {
        if (!fromSelf()) {
            return;
        }

        try {
            sockets.get(id)?.send(text);
        } catch {
        }
    });

    on(prefix + 'WsClose', (id) => {
        if (!fromSelf()) {
            return;
        }

        const ws = sockets.get(id);
        sockets.delete(id);

        try {
            ws?.close();
        } catch {
        }
    });
}
