---
title: "Webhook Logging"
description: "Send a record of what happens on your server to Discord, or to your own tooling as plain JSON."
---

vMenu can post a running record of what happens on your server to Discord, or to your own tooling as plain JSON. It is off by default and nothing is sent until you fill in a webhook URL.

## Discord

First make a webhook in the Discord channel you want the logs to go to. Discord explains how to do that here: [Intro to Webhooks](https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks). Copy the webhook URL it gives you.

Then paste it into your `configuration.cfg`:

```ini
setr vMenu.Enhanced.Logging.Enabled true

set vMenu.Enhanced.Logging.Webhook.Events "https://discord.com/api/webhooks/..."
set vMenu.Enhanced.Logging.Webhook.Actions "https://discord.com/api/webhooks/..."
set vMenu.Enhanced.Logging.Webhook.Staff "https://discord.com/api/webhooks/..."
set vMenu.Enhanced.Logging.Webhook.Security "https://discord.com/api/webhooks/..."
```

Each one is a separate channel. Point them all at one webhook, use some, or leave the ones you do not want empty. Leaving Security empty sends its lines to the Staff webhook instead.

:::caution[`set`, not `setr`]
The webhook URL is a password. Anybody who has it can post to your channel. Use plain `set` so it stays on the server. `setr` would hand it to every player who joins.
:::

## Generic

If you would rather send the logs to your own tooling instead of Discord, point the generic webhook at your own endpoint:

```ini
set vMenu.Enhanced.Logging.Webhook.Generic "http://127.0.0.1:3000/vmenu"
```

It receives plain JSON, in batches, shaped like this:

```json
{
  "resource": "vMenu.Enhanced",
  "version": "1.2.3",
  "sentAt": "2026-09-13T20:36:00.1200000+00:00",
  "events": [
    {
      "category": "event",
      "message": "was kicked from the server.",
      "timestamp": "2026-09-13T20:35:58.4000000+00:00",
      "actor": {
        "name": "Bob",
        "serverId": 9,
        "identifiers": { "discord": "123456789", "license": "abc123", "license2": "def456", "fivem": "1234567" }
      },
      "targets": [],
      "data": { "cause": "kick", "origin": "menu", "by": "Tom", "reason": "Breaking the rules." }
    }
  ]
}
```

`category` is `event`, `action`, `staff` or `security`.

The `actor` is who the line is about, so on a join, a leave, or a kick it is the player who joined, left, or was kicked. `targets` holds anyone that player then acted on, which is why a leave or a kick has none, while a staff member freezing somebody lists the frozen player there. The `identifiers` object holds every identifier the actor has, apart from their IP address, which is never sent, and only lists the ones that exist, so a player with just a license shows just a license.

`data` is any extra context for the line, and is empty when there is none. When somebody leaves, `cause` says why: `left`, `timeout`, `server`, `replaced`, `onesync`, `rate limit`, or `kick`. When it is a kick, `origin` says what did the kicking, either `menu`, `integration`, or the name of another resource that dropped the player, and for a menu or integration kick `by` names the staff member and `reason` gives their reason.

### Every kind of line

For a file with an example of every category and every shape of `data` vMenu can send, download [webhook-log-examples.json](/vmenu/enhanced/webhook-log-examples.json). It is handy to look at while you are building something that reads the generic webhook.
