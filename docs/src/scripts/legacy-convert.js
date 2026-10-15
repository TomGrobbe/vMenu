import { unpack } from "./msgpack.js";
import { LEGACY_TIMECYCLES } from "./legacy-timecycles.js";

const OLD_KEY = /^(veh_|ped_|mp_ped_|settings_|saved_veh_category_|mp_character_category_|vmenu_string_saved_weapon_loadout_|vmenu_default_character$|vmenu_string_default_loadout$)/;

const VEHICLE_VERSION = 2;
const PED_VERSION = 2;
const CHARACTER_VERSION = 2;
const LOADOUT_VERSION = 1;
const SETTING_VERSION = 1;
const TIMECYCLE_VERSION = 2;

const SAVE_PREFIXES = ["veh_", "ped_", "mp_ped_", "vmenu_string_saved_weapon_loadout_"];
const TRAILING_SPACES = / +$/;
const IMPORTED_CATEGORY = "Imported from old vMenu";
const IMPORTED_DESCRIPTION = "Brought over from the original vMenu.";
const UNPAINTED = 7;
const STOCK_HEADLIGHT = 255;
const PROP_SLOTS = [0, 1, 2, 6, 7];
const CORE_OVERLAYS = [0, 3, 6, 7, 9, 11];

const OVERLAYS = [
    { slot: 0, style: "blemishesStyle", opacity: "blemishesOpacity" },
    { slot: 1, style: "beardStyle", opacity: "beardOpacity", colour: "beardColor" },
    { slot: 2, style: "eyebrowsStyle", opacity: "eyebrowsOpacity", colour: "eyebrowsColor" },
    { slot: 3, style: "ageingStyle", opacity: "ageingOpacity" },
    { slot: 4, style: "makeupStyle", opacity: "makeupOpacity", colour: "makeupColor" },
    { slot: 5, style: "blushStyle", opacity: "blushOpacity", colour: "blushColor" },
    { slot: 6, style: "complexionStyle", opacity: "complexionOpacity" },
    { slot: 7, style: "sunDamageStyle", opacity: "sunDamageOpacity" },
    { slot: 8, style: "lipstickStyle", opacity: "lipstickOpacity", colour: "lipstickColor" },
    { slot: 9, style: "molesFrecklesStyle", opacity: "molesFrecklesOpacity" },
    { slot: 10, style: "chestHairStyle", opacity: "chestHairOpacity", colour: "chestHairColor" },
    { slot: 11, style: "bodyBlemishesStyle", opacity: "bodyBlemishesOpacity" },
];

const TATTOO_ZONES = [
    ["HairTattoos", "hair"],
    ["HeadTattoos", "head"],
    ["TorsoTattoos", "torso"],
    ["LeftArmTattoos", "leftArm"],
    ["RightArmTattoos", "rightArm"],
    ["LeftLegTattoos", "leftLeg"],
    ["RightLegTattoos", "rightLeg"],
    ["BadgeTattoos", "badges"],
    ["AddonTattoos", "addons"],
];

const SAME_BOOLS = [
    "playerGodMode",
    "playerStayInVehicle",
    "vehicleGodMode",
    "vehicleGodInvincible",
    "vehicleGodEngine",
    "vehicleGodVisual",
    "vehicleGodStrongWheels",
    "vehicleGodRamp",
    "vehicleGodAutoRepair",
    "vehicleEngineAlwaysOn",
    "vehicleAnchorBoat",
    "vehicleSpawnerSpawnInside",
    "vehicleSpawnerReplacePrevious",
    "weaponsNoReload",
    "weaponsUnlimitedAmmo",
    "weaponsUnlimitedParachutes",
];

const RENAMED_BOOLS = {
    fastRun: "playerFastRun",
    fastSwim: "playerFastSwim",
    superJump: "playerSuperJump",
    noRagdoll: "playerNoRagdoll",
    neverWanted: "playerNeverWanted",
    everyoneIgnorePlayer: "playerEveryoneIgnores",
    vehicleNeverDirty: "vehicleKeepClean",
    vehicleNoBikeHelmet: "playerNoHelmet",
    vehicleNoBikeHelemet: "playerNoHelmet",
    autoEquipParachuteWhenInPlane: "weaponsAutoEquipParachute",
    miscJoinQuitNotifications: "displayJoinLeaveNotifications",
    miscDeathNotifications: "displayDeathNotifications",
    miscShowLocation: "displayShowLocation",
    miscLocationBlips: "displayLocationBlips",
    miscShowPlayerBlips: "displayShowPlayerBlips",
    miscShowOverheadNames: "displayShowOverheadNames",
    miscShowTime: "displayShowTime",
    miscRightAlignMenu: "displayRightAlignMenu",
    weaponLoadoutsSetLoadoutOnRespawn: "weaponLoadoutOnRespawn",
    pvEnableVehicleBlip: "personalVehicleBlip",
};

const HANDLED_ELSEWHERE = new Set([
    "miscSpeedoKmh",
    "miscSpeedoMph",
    "vehicleDisablePlaneTurbulence",
    "vehicleDisableHelicopterTurbulence",
    "clothingAnimationType",
    "miscLastTimeCycleModifierIndex",
    "miscLastTimeCycleModifierStrength",
    "miscRespawnDefaultCharacter",
]);

const OLD_GLOW_TO_NEW = { 0: 1, 1: 0, 2: 2, 3: 3 };

export const GROUP_ORDER = [
    "Saved vehicles",
    "Vehicle categories",
    "Saved peds",
    "Ped categories",
    "Custom characters",
    "Character categories",
    "Weapon loadouts",
    "Settings",
];

export function joaat(text) {
    const lower = String(text).toLowerCase();
    let hash = 0;

    for (let index = 0; index < lower.length; index++) {
        hash = (hash + lower.charCodeAt(index)) >>> 0;
        hash = (hash + (hash << 10)) >>> 0;
        hash = (hash ^ (hash >>> 6)) >>> 0;
    }

    hash = (hash + (hash << 3)) >>> 0;
    hash = (hash ^ (hash >>> 11)) >>> 0;
    hash = (hash + (hash << 15)) >>> 0;

    return hash;
}

export function parseCommentedJson(text) {
    let out = "";
    let inString = false;

    for (let at = 0; at < text.length; at++) {
        const char = text[at];

        if (inString) {
            out += char;

            if (char === "\\") {
                out += text[++at] || "";
            } else if (char === '"') {
                inString = false;
            }

            continue;
        }

        if (char === '"') {
            inString = true;
            out += char;
        } else if (char === "/" && text[at + 1] === "/") {
            while (at < text.length && text[at] !== "\n") at++;
            out += "\n";
        } else if (char === "/" && text[at + 1] === "*") {
            at = text.indexOf("*/", at + 2);
            at = at < 0 ? text.length : at + 1;
        } else {
            out += char;
        }
    }

    return JSON.parse(out.replace(/,(\s*[}\]])/g, "$1"));
}

export function makeLookups({ pedModels, weapons, weaponComponents }) {
    const pedNames = new Map();
    const weaponNames = new Set();
    const componentNames = new Map();

    for (const group of Object.values(pedModels || {})) {
        for (const model of Object.keys(group || {})) {
            pedNames.set(joaat(model), model.toLowerCase());
        }
    }

    for (const group of Object.values(weapons || {})) {
        for (const weapon of Object.keys(group || {})) {
            weaponNames.add(weapon.toLowerCase());
        }
    }

    for (const component of Object.keys(weaponComponents || {})) {
        componentNames.set(joaat(component), component);
    }

    return { pedNames, weaponNames, componentNames };
}

function int(value, fallback = 0) {
    const number = Number(value);

    return Number.isFinite(number) ? Math.trunc(number) : fallback;
}

function float(value, fallback = 0, low = -Infinity, high = Infinity) {
    const number = Number(value);

    if (!Number.isFinite(number)) {
        return fallback;
    }

    return Math.min(high, Math.max(low, Math.round(number * 1000) / 1000));
}

function bool(value) {
    return value === true || String(value).toLowerCase() === "true";
}

function text(value) {
    return typeof value === "string" ? value : "";
}

function envelope(key, type, version, value) {
    return JSON.stringify({ key, value, type, version });
}

function decode(bytes) {
    try {
        return { value: unpack(bytes) };
    } catch {
        return { error: "The stored value could not be unpacked." };
    }
}

function json(value) {
    if (typeof value !== "string") {
        return { error: "The stored value is not text." };
    }

    try {
        const parsed = JSON.parse(value);

        return parsed && typeof parsed === "object" ? { value: parsed } : { error: "The stored value is not a saved item." };
    } catch {
        return { error: "The stored text is damaged and could not be read." };
    }
}

function isUncategorized(name) {
    return !name || name.trim().length === 0 || name.trim().toLowerCase() === "uncategorized";
}

export function findResources(entries) {
    const counts = new Map();

    for (const key of entries.keys()) {
        const match = /^res:([^:]+):(.*)$/s.exec(key);

        if (match && OLD_KEY.test(match[2])) {
            counts.set(match[1], (counts.get(match[1]) || 0) + 1);
        }
    }

    return [...counts.entries()]
        .map(([name, count]) => ({ name, count }))
        .sort((left, right) => right.count - left.count);
}

function vehicleFrom(name, old) {
    const colors = old.colors && typeof old.colors === "object" ? old.colors : {};
    const channel = (prefix, red, green, blue) => {
        const values = [colors[red] ?? old[red], colors[green] ?? old[green], colors[blue] ?? old[blue]];

        return values.every(value => Number.isFinite(Number(value)) && Number(value) >= 0)
            ? { [prefix + "Red"]: int(values[0]), [prefix + "Green"]: int(values[1]), [prefix + "Blue"]: int(values[2]) }
            : { [prefix + "Red"]: null, [prefix + "Green"]: null, [prefix + "Blue"]: null };
    };

    const mods = Object.entries(old.mods && typeof old.mods === "object" ? old.mods : {})
        .map(([slot, value]) => ({ slot: int(slot, -1), value: int(value, -1) }))
        .filter(mod => mod.slot >= 0 && mod.slot <= 49 && (mod.slot < 17 || mod.slot > 22))
        .sort((left, right) => left.slot - right.slot);

    const extras = Object.entries(old.extras && typeof old.extras === "object" ? old.extras : {})
        .map(([id, on]) => ({ id: int(id, -1), on: bool(on) }))
        .filter(extra => extra.id >= 0 && extra.id < 20)
        .sort((left, right) => left.id - right.id);

    const headlight = int(old.headlightColor, -1);

    return {
        name,
        description: "",
        category: isUncategorized(text(old.Category)) ? IMPORTED_CATEGORY : old.Category.trim(),
        appearance: {
            modelName: "",
            modelHash: int(old.model) >>> 0,
            mods,
            customTyres: bool(old.customWheels),
            wheelType: int(old.wheelType),
            turbo: bool(old.turbo),
            tyreSmoke: bool(old.tyreSmoke),
            xenonLights: bool(old.xenonHeadlights),
            bulletproofTyres: bool(old.bulletProofTires),
            driftTyres: false,
            primaryPaintType: old.PrimaryPaintFinish === undefined ? UNPAINTED : int(old.PrimaryPaintFinish, UNPAINTED),
            primaryColor: int(colors.primary),
            secondaryPaintType: old.SecondaryPaintFinish === undefined ? UNPAINTED : int(old.SecondaryPaintFinish, UNPAINTED),
            secondaryColor: int(colors.secondary),
            pearlescentColor: int(colors.pearlescent),
            wheelColor: int(colors.wheels),
            dashboardColor: int(colors.dash),
            interiorColor: int(colors.trim),
            ...channel("customPrimary", "customPrimaryR", "customPrimaryG", "customPrimaryB"),
            ...channel("customSecondary", "customSecondaryR", "customSecondaryG", "customSecondaryB"),
            paintFade: float(old.enveffScale, 0, 0, 1),
            headlightColor: headlight >= 0 && headlight <= 12 ? headlight : STOCK_HEADLIGHT,
            ...channel("customXenon", "customheadlightR", "customheadlightG", "customheadlightB"),
            neonFront: bool(old.neonFront),
            neonRear: bool(old.neonBack),
            neonLeft: bool(old.neonLeft),
            neonRight: bool(old.neonRight),
            neonRed: int(colors.neonR, 255),
            neonGreen: int(colors.neonG, 255),
            neonBlue: int(colors.neonB, 255),
            tyreSmokeRed: int(colors.tyresmokeR, 255),
            tyreSmokeGreen: int(colors.tyresmokeG, 255),
            tyreSmokeBlue: int(colors.tyresmokeB, 255),
            livery: int(old.livery, -1),
            roofLivery: -1,
            extras,
            windowTint: int(old.windowTint, -1),
            plateText: text(old.plateText),
            plateStyle: int(old.plateStyle),
            dirtLevel: 0,
        },
    };
}

function propsFrom(source, read) {
    const props = [];

    for (const slot of PROP_SLOTS) {
        const prop = read(source, slot);

        if (prop && prop.drawable >= 0) {
            props.push({ slot, drawable: prop.drawable, texture: Math.max(0, prop.texture), collection: "", localDrawable: -1 });
        }
    }

    return props;
}

function pedFrom(name, old, lookups) {
    const drawables = old.drawableVariations || {};
    const textures = old.drawableVariationTextures || {};
    const components = [];

    for (let slot = 0; slot <= 11; slot++) {
        if (drawables[slot] === undefined) {
            continue;
        }

        components.push({
            slot,
            drawable: Math.max(0, int(drawables[slot])),
            texture: Math.max(0, int(textures[slot])),
            palette: 0,
            collection: "",
            localDrawable: -1,
        });
    }

    const props = propsFrom(old, (source, slot) => {
        const drawable = int((source.props || {})[slot], -1);
        const texture = int((source.propTextures || {})[slot], -1);

        return { drawable: texture < 0 ? -1 : drawable, texture };
    });

    const hash = int(old.model) >>> 0;

    return {
        name,
        description: "",
        category: IMPORTED_CATEGORY,
        appearance: { modelName: lookups.pedNames.get(hash) || "", modelHash: hash, components, props },
        movementClipset: "",
    };
}

function pairs(list) {
    return (Array.isArray(list) ? list : [])
        .map(pair => ({ collection: text(pair && pair.Key), name: text(pair && pair.Value) }))
        .filter(pair => pair.collection && pair.name);
}

function characterFrom(name, old) {
    const blend = old.PedHeadBlendData || {};
    const looks = old.PedAppearance || {};
    const features = (old.FaceShapeFeatures && old.FaceShapeFeatures.features) || {};
    const clothes = (old.DrawableVariations && old.DrawableVariations.clothes) || {};
    const oldProps = (old.PropVariations && old.PropVariations.props) || {};

    const overlay = spec => ({
        slot: spec.slot,
        style: Math.max(0, int(looks[spec.style])),
        opacity: float(looks[spec.opacity], 0, 0, 1),
        colour: spec.colour ? Math.max(0, int(looks[spec.colour])) : 0,
    });

    const tattoos = {};

    for (const [from, to] of TATTOO_ZONES) {
        tattoos[to] = pairs((old.PedTatttoos || {})[from]);
    }

    const components = [];

    for (let slot = 0; slot <= 11; slot++) {
        const piece = clothes[slot];

        if (!piece || int(piece.Key, -1) < 0) {
            continue;
        }

        components.push({
            slot,
            drawable: int(piece.Key),
            texture: Math.max(0, int(piece.Value)),
            palette: 0,
            collection: "",
            localDrawable: -1,
        });
    }

    const props = propsFrom(oldProps, (source, slot) => {
        const piece = source[slot];

        return piece ? { drawable: int(piece.Key, -1), texture: int(piece.Value, 0) } : null;
    });

    const hairPiece = clothes[2];
    const hairOverlay = looks.HairOverlay || {};
    const hasDecoration = text(hairOverlay.Key) && text(hairOverlay.Value);

    const style = {
        name: "Default",
        description: "",
        hairStyle: hairPiece && int(hairPiece.Key, -1) >= 0 ? int(hairPiece.Key) : Math.max(0, int(looks.hairStyle)),
        hairColour: Math.max(0, int(looks.hairColor)),
        hairHighlight: Math.max(0, int(looks.hairHighlightColor)),
        hairDecorationCollection: hasDecoration ? hairOverlay.Key : "",
        hairDecorationName: hasDecoration ? hairOverlay.Value : "",
        overlays: OVERLAYS.filter(spec => !CORE_OVERLAYS.includes(spec.slot)).map(overlay),
    };

    const outfit = { name: "Default", description: "", outfit: { components, props } };

    return {
        name,
        description: "",
        category: isUncategorized(text(old.Category)) ? IMPORTED_CATEGORY : old.Category.trim(),
        core: {
            isMale: old.IsMale === undefined ? true : bool(old.IsMale),
            blend: {
                firstShape: Math.max(0, int(blend.FirstFaceShape)),
                secondShape: Math.max(0, int(blend.SecondFaceShape)),
                thirdShape: Math.max(0, int(blend.ThirdFaceShape)),
                firstSkin: Math.max(0, int(blend.FirstSkinTone)),
                secondSkin: Math.max(0, int(blend.SecondSkinTone)),
                thirdSkin: Math.max(0, int(blend.ThirdSkinTone)),
                shapeMix: float(blend.ParentFaceShapePercent, 0.5, 0, 1),
                skinMix: float(blend.ParentSkinTonePercent, 0.5, 0, 1),
                thirdMix: 0,
                isParent: bool(blend.IsParentInheritance),
            },
            faceFeatures: Array.from({ length: 20 }, (_, index) => float(features[index], 0, -1, 1)),
            eyeColour: Math.max(0, int(looks.eyeColor)),
            overlays: OVERLAYS.filter(spec => CORE_OVERLAYS.includes(spec.slot)).map(overlay),
            tattoos,
        },
        currentStyle: style,
        currentOutfit: outfit,
        styles: [style],
        outfits: [outfit],
        lastStyle: style.name,
        lastOutfit: outfit.name,
        facialExpression: text(old.FacialExpression) || "mood_Normal_1",
        movementClipset: "",
    };
}

function loadoutFrom(name, old, lookups) {
    const notes = [];
    const weapons = [];
    let lostComponents = 0;

    for (const weapon of old) {
        const spawnName = text(weapon && weapon.SpawnName).toLowerCase();

        if (!spawnName) {
            continue;
        }

        if (lookups.weaponNames.size > 0 && !lookups.weaponNames.has(spawnName)) {
            notes.push(spawnName + " is not a standard weapon. It only loads if the server has it.");
        }

        const components = [];

        for (const hash of Object.values((weapon && weapon.Components) || {})) {
            const known = lookups.componentNames.get(int(hash) >>> 0);

            if (known) {
                components.push(known);
            } else {
                lostComponents++;
            }
        }

        const ammo = int(weapon.CurrentAmmo, -1);

        weapons.push({
            spawnName,
            ammo: ammo < 0 ? Math.max(0, int(weapon.GetMaxAmmo, 250)) : ammo,
            tint: Math.max(0, int(weapon.CurrentTint)),
            components,
        });
    }

    if (lostComponents > 0) {
        notes.push(lostComponents + " attachment(s) could not be recognised and were left off.");
    }

    return { value: { name, weapons }, notes };
}

function categoryEntry(name) {
    return { name, description: name === IMPORTED_CATEGORY ? IMPORTED_DESCRIPTION : "" };
}

function categoryFrom(old) {
    return { name: text(old.Name).trim(), description: text(old.Description) };
}

export function convert(entries, resource, lookups) {
    const prefix = "res:" + resource + ":";
    const items = [];
    const old = new Map();

    for (const [key, bytes] of entries) {
        if (key.startsWith(prefix)) {
            old.set(key.slice(prefix.length), bytes);
        }
    }

    const add = (group, name, key, type, version, value, notes = []) => {
        const item = { group, name, key, raw: envelope(key, type, version, value), notes, skipped: null, requires: null };

        items.push(item);

        return item;
    };

    const skip = (group, name, reason) => {
        items.push({ group, name, key: null, raw: null, notes: [], skipped: reason });
    };

    const readJson = (oldKey, group, name) => {
        const decoded = decode(old.get(oldKey));

        if (decoded.error) {
            skip(group, name, decoded.error);

            return null;
        }

        const parsed = json(decoded.value);

        if (parsed.error) {
            skip(group, name, parsed.error);

            return null;
        }

        return parsed.value;
    };

    const vehicleCategories = new Map();
    let anyPed = false;
    const characterCategories = new Map();

    for (const oldKey of [...old.keys()].sort()) {
        if (oldKey.startsWith("saved_veh_category_") || oldKey.startsWith("mp_character_category_")) {
            const isVehicle = oldKey.startsWith("saved_veh_category_");
            const label = isVehicle ? "Vehicle categories" : "Character categories";
            const value = readJson(oldKey, label, oldKey.replace(/^(saved_veh_category_|mp_character_category_)/, ""));

            if (value) {
                const category = categoryFrom(value);

                if (!isUncategorized(category.name)) {
                    (isVehicle ? vehicleCategories : characterCategories).set(category.name.toLowerCase(), category);
                }
            }
        }
    }

    const characters = new Map();
    const loadouts = new Map();
    const taken = new Map(SAVE_PREFIXES.map(prefix => [prefix, new Set()]));

    for (const oldKey of old.keys()) {
        const prefix = SAVE_PREFIXES.find(start => oldKey.startsWith(start));

        if (prefix && !TRAILING_SPACES.test(oldKey)) {
            taken.get(prefix).add(oldKey.slice(prefix.length));
        }
    }

    const saveName = (prefix, name, notes) => {
        if (!TRAILING_SPACES.test(name)) {
            return name;
        }

        const used = taken.get(prefix);
        let next = name.replace(TRAILING_SPACES, spaces => "-".repeat(spaces.length));

        while (used.has(next)) {
            next += "-";
        }

        used.add(next);
        notes.push("Its old name ended in a space, so it is saved as " + next + " instead.");

        return next;
    };

    for (const oldKey of [...old.keys()].sort((left, right) => left.localeCompare(right))) {
        if (oldKey.startsWith("veh_")) {
            const name = oldKey.slice(4);
            const value = name.trim() ? readJson(oldKey, "Saved vehicles", name) : null;

            if (!name.trim()) {
                skip("Saved vehicles", oldKey, "This save has no name.");
            } else if (value) {
                if (value.version === undefined || !value.colors) {
                    skip("Saved vehicles", name, "Saved by a very old vMenu, from before 2018, in a format that cannot be converted.");
                } else if (!int(value.model)) {
                    skip("Saved vehicles", name, "This save has no vehicle model in it.");
                } else {
                    const notes = [];
                    const vehicle = vehicleFrom(saveName("veh_", name, notes), value);

                    if (vehicle.category && !vehicleCategories.has(vehicle.category.toLowerCase())) {
                        vehicleCategories.set(vehicle.category.toLowerCase(), categoryEntry(vehicle.category));
                    }

                    add("Saved vehicles", vehicle.name, "vmenu_vehicle_" + vehicle.name, "json", VEHICLE_VERSION, vehicle, notes);
                }
            }
        } else if (oldKey.startsWith("ped_")) {
            const name = oldKey.slice(4);
            const value = name.trim() ? readJson(oldKey, "Saved peds", name) : null;

            if (!name.trim()) {
                skip("Saved peds", oldKey, "This save has no name.");
            } else if (value) {
                if (int(value.version) !== 1 || !int(value.model)) {
                    skip("Saved peds", name, "Saved by a very old vMenu, in a format that cannot be converted.");
                } else {
                    const notes = value.isMpPed
                        ? ["This is a multiplayer ped saved as a normal ped, so it has no face or hair. It will look plain."]
                        : [];
                    const ped = pedFrom(saveName("ped_", name, notes), value, lookups);

                    anyPed = true;
                    add("Saved peds", ped.name, "vmenu_ped_" + ped.name, "json", PED_VERSION, ped, notes);
                }
            }
        } else if (oldKey.startsWith("mp_ped_")) {
            const name = oldKey.slice(7);
            const value = name.trim() ? readJson(oldKey, "Custom characters", name) : null;

            if (!name.trim()) {
                skip("Custom characters", oldKey, "This save has no name.");
            } else if (value) {
                if (!value.PedHeadBlendData || !value.PedAppearance) {
                    skip("Custom characters", name, "This save is missing its face or appearance data.");
                } else {
                    const notes = ["Clothes are matched by number, so a DLC piece can come out as a different one. Save it once in game to lock it in."];
                    const character = characterFrom(saveName("mp_ped_", name, notes), value);

                    if (character.category && !characterCategories.has(character.category.toLowerCase())) {
                        characterCategories.set(character.category.toLowerCase(), categoryEntry(character.category));
                    }

                    characters.set(name, character.name);
                    add("Custom characters", character.name, "vmenu_mpchar_" + character.name, "json", CHARACTER_VERSION, character, notes);
                }
            }
        } else if (oldKey.startsWith("vmenu_string_saved_weapon_loadout_")) {
            const name = oldKey.slice("vmenu_string_saved_weapon_loadout_".length);
            const value = name.trim() ? readJson(oldKey, "Weapon loadouts", name) : null;

            if (!name.trim()) {
                skip("Weapon loadouts", oldKey, "This save has no name.");
            } else if (value) {
                if (!Array.isArray(value)) {
                    skip("Weapon loadouts", name, "This save is not a list of weapons.");
                } else {
                    const loadout = loadoutFrom(saveName("vmenu_string_saved_weapon_loadout_", name, []), value, lookups);
                    const saved = loadout.value.name;

                    if (saved !== name) {
                        loadout.notes.unshift("Its old name ended in a space, so it is saved as " + saved + " instead.");
                    }

                    loadouts.set(name, saved);
                    add("Weapon loadouts", saved, "vmenu_weaponloadout_" + saved, "json", LOADOUT_VERSION, loadout.value, loadout.notes);
                }
            }
        }
    }

    for (const category of vehicleCategories.values()) {
        add("Vehicle categories", category.name, "vmenu_vehcategory_" + category.name, "json", VEHICLE_VERSION, category);
    }

    if (anyPed) {
        add("Ped categories", IMPORTED_CATEGORY, "vmenu_pedcategory_" + IMPORTED_CATEGORY, "json", PED_VERSION, categoryEntry(IMPORTED_CATEGORY));
    }

    for (const category of characterCategories.values()) {
        add("Character categories", category.name, "vmenu_mpcharcategory_" + category.name, "json", CHARACTER_VERSION, category);
    }

    linkCategories(items);
    convertSettings(old, add, skip, characters, loadouts);

    items.sort((left, right) => GROUP_ORDER.indexOf(left.group) - GROUP_ORDER.indexOf(right.group));

    return items;
}

const CATEGORY_MEMBERS = {
    "Vehicle categories": "Saved vehicles",
    "Ped categories": "Saved peds",
    "Character categories": "Custom characters",
};

function linkCategories(items) {
    for (const category of items) {
        const members = CATEGORY_MEMBERS[category.group];

        if (!members) {
            continue;
        }

        const wanted = category.name.toLowerCase();

        category.requires = items
            .filter(item => item.group === members && item.raw && JSON.parse(item.raw).value.category.toLowerCase() === wanted)
            .map(item => item.key);
    }
}

function convertSettings(old, add, skip, characters, loadouts) {
    const read = name => {
        const bytes = old.get(name);

        if (!bytes) {
            return undefined;
        }

        const decoded = decode(bytes);

        return decoded.error ? undefined : decoded.value;
    };

    const written = new Set();

    const setting = (name, type, value, from) => {
        if (written.has(name)) {
            return null;
        }

        written.add(name);
        return add("Settings", name, "vmenu_default_" + name, type, SETTING_VERSION, value, from ? ["From the old " + from + " setting."] : []);
    };

    for (const name of SAME_BOOLS) {
        const value = read("settings_" + name);

        if (value !== undefined) {
            setting(name, "bool", bool(value), name);
        }
    }

    for (const [from, to] of Object.entries(RENAMED_BOOLS)) {
        const value = read("settings_" + from);

        if (value !== undefined) {
            setting(to, "bool", bool(value), from);
        }
    }

    const kmh = read("settings_miscSpeedoKmh");
    const mph = read("settings_miscSpeedoMph");

    if (kmh !== undefined || mph !== undefined) {
        setting("displaySpeedometer", "int", (bool(kmh) ? 1 : 0) + (bool(mph) ? 2 : 0), "speedometer");
    }

    for (const [from, to] of [["vehicleDisablePlaneTurbulence", "vehiclePlaneTurbulence"], ["vehicleDisableHelicopterTurbulence", "vehicleHeliTurbulence"]]) {
        const value = read("settings_" + from);

        if (value !== undefined) {
            setting(to, "int", bool(value) ? 0 : 100, from);
        }
    }

    const glow = read("settings_clothingAnimationType");

    if (glow !== undefined && OLD_GLOW_TO_NEW[int(glow, -1)] !== undefined) {
        setting("playerClothingGlow", "int", OLD_GLOW_TO_NEW[int(glow)], "clothingAnimationType");
    }

    const cycle = read("settings_miscLastTimeCycleModifierIndex");
    const strength = read("settings_miscLastTimeCycleModifierStrength");
    const cycleName = cycle === undefined ? undefined : LEGACY_TIMECYCLES[int(cycle, -1)];

    if (cycleName) {
        const intensity = Math.min(20, Math.max(0, int(strength, 10)));

        add("Settings", "Time cycle", "vmenu_timecycles", "json", TIMECYCLE_VERSION, { names: [cycleName], intensity }, [
            "Your last time cycle, " + cycleName + ".",
        ]);
    }

    const defaultCharacter = text(read("vmenu_default_character")).replace(/^mp_ped_/, "");
    const respawnAsDefault = read("settings_miscRespawnDefaultCharacter");

    if (defaultCharacter) {
        if (respawnAsDefault !== undefined && !bool(respawnAsDefault)) {
            skip("Settings", "defaultCharacterName", "Your default character was turned off in the old vMenu, so it is left unset.");
        } else if (!characters.has(defaultCharacter)) {
            skip("Settings", "defaultCharacterName", "Your default character, " + defaultCharacter + ", is no longer saved.");
        } else {
            const saved = characters.get(defaultCharacter);

            setting("defaultCharacterName", "string", saved, "default character").requires = ["vmenu_mpchar_" + saved];
        }
    }

    const defaultLoadout = text(read("vmenu_string_default_loadout")).replace(/^vmenu_string_saved_weapon_loadout_/, "");

    if (defaultLoadout) {
        if (loadouts.has(defaultLoadout)) {
            const saved = loadouts.get(defaultLoadout);

            setting("weaponLoadoutDefaultName", "string", saved, "default loadout").requires = ["vmenu_weaponloadout_" + saved];
        } else {
            skip("Settings", "weaponLoadoutDefaultName", "Your default loadout, " + defaultLoadout + ", is no longer saved.");
        }
    }

    for (const key of old.keys()) {
        if (!key.startsWith("settings_")) {
            continue;
        }

        const name = key.slice("settings_".length);

        if (SAME_BOOLS.includes(name) || RENAMED_BOOLS[name] || HANDLED_ELSEWHERE.has(name)) {
            continue;
        }

        skip("Settings", name, "vMenu Enhanced has no setting like this one.");
    }
}
