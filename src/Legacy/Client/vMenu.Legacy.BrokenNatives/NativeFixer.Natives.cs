using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace vMenu.Enhanced.BrokenNatives;

public static partial class NativeFixer
{
    public static void ActivateRockstarEditor(int eOpenedFromSource)
        => Function.Call((Hash)0x49DA8145672B2725UL, eOpenedFromSource);

    public static void AddAmmoToPedByType(int pedIndex, uint ammoType, int ammoAmount)
        => API.AddAmmoToPedByType(pedIndex, unchecked((int)ammoType), ammoAmount);

    public static void AddDoorToSystem(uint doorEnumHash, uint objectModelHashKey, float vecPos_x, float vecPos_y, float vecPos_z, bool useOldOverrides, bool network, bool permanent, bool runTimeCreatedDoor)
        => Function.Call((Hash)0x6F8838D03D1DC226UL, doorEnumHash, objectModelHashKey, vecPos_x, vecPos_y, vecPos_z, useOldOverrides, network, permanent, runTimeCreatedDoor);

    public static void AddExplosion(float vecExplosionPos_x, float vecExplosionPos_y, float vecExplosionPos_z, int explosionTag, float sizeScale, bool bMakeSound, bool bNoFx, float camShakeMultiplier, bool noDamage)
        => Function.Call((Hash)0xE3AD2BDBAEE269ACUL, vecExplosionPos_x, vecExplosionPos_y, vecExplosionPos_z, explosionTag, sizeScale, bMakeSound, bNoFx, camShakeMultiplier, noDamage);

    public static bool AddTextEntry(string p0, string p1)
    {
        API.AddTextEntry(p0, p1);
        return true;
    }

    public static bool AddTextEntryByHash(uint p0, string p1)
    {
        API.AddTextEntryByHash(p0, p1);
        return true;
    }

    public static void AttachEntityToEntity(int firstEntityIndex, int secondEntityIndex, int secondEntityBoneIndex, float vecOffset_x, float vecOffset_y, float vecOffset_z, float vecRotation_x, float vecRotation_y, float vecRotation_z, bool bDetachWhenDead, bool bDetachWhenRagdoll, bool bActiveCollisions, bool bUseBasicAttachIfPed, int rotOrder, bool bAttachOffsetIsRelative, bool bMarkAsNoLongerNeededWhenDetached)
        => Function.Call((Hash)0x6B9BBD38AB0796DFUL, firstEntityIndex, secondEntityIndex, secondEntityBoneIndex, vecOffset_x, vecOffset_y, vecOffset_z, vecRotation_x, vecRotation_y, vecRotation_z, bDetachWhenDead, bDetachWhenRagdoll, bActiveCollisions, bUseBasicAttachIfPed, rotOrder, bAttachOffsetIsRelative, bMarkAsNoLongerNeededWhenDetached);

    public static void ClearAllPedProps(int pedIndex, bool doDeadCheck)
        => Function.Call((Hash)0xCD8A7537A9B52F06UL, pedIndex, doDeadCheck);

    public static void ClearPedProp(int pedIndex, int position, bool doDeadCheck)
        => Function.Call((Hash)0x0943E5B8E078E76EUL, pedIndex, position, doDeadCheck);

    public static void ClearVehicleCustomPrimaryColour(int vehicleIndex)
        => API.ClearVehicleCustomPrimaryColour(vehicleIndex);

    public static void ClearVehicleCustomSecondaryColour(int vehicleIndex)
        => API.ClearVehicleCustomSecondaryColour(vehicleIndex);

    public static int ClonePed(int pedIndex, bool bRegisterAsNetworkObject, bool bScriptHostObject, bool bLinkBlends)
        => Function.Call<int>((Hash)0xEF29A16337FACADBUL, pedIndex, bRegisterAsNetworkObject, bScriptHostObject, bLinkBlends);

    public static int CreateObject(uint modelIndex, float vecNewCoors_x, float vecNewCoors_y, float vecNewCoors_z, bool registerAsNetworkObject, bool scriptHostObject, bool forceToBeObject)
        => API.CreateObject(unchecked((int)modelIndex), vecNewCoors_x, vecNewCoors_y, vecNewCoors_z, registerAsNetworkObject, scriptHostObject, forceToBeObject);

    public static int CreateObjectNoOffset(uint modelIndex, float vecNewCoors_x, float vecNewCoors_y, float vecNewCoors_z, bool registerAsNetworkObject, bool scriptHostObject, bool forceToBeObject, bool forceToBeDoor)
        => Function.Call<int>((Hash)0x9A294B2138ABB884UL, modelIndex, vecNewCoors_x, vecNewCoors_y, vecNewCoors_z, registerAsNetworkObject, scriptHostObject, forceToBeObject, forceToBeDoor);

    public static int CreateVehicle(uint modelHashKey, float vecCoors_x, float vecCoors_y, float vecCoors_z, float fVehicleHeading, bool registerAsNetworkObject, bool scriptHostObject, bool bIgnoreGroundCheck)
        => Function.Call<int>((Hash)0xAF35D0D2583051B0UL, modelHashKey, vecCoors_x, vecCoors_y, vecCoors_z, fVehicleHeading, registerAsNetworkObject, scriptHostObject, bIgnoreGroundCheck);

    public static void DeleteEntity(ref int entityIndex)
        => API.DeleteEntity(ref entityIndex);

    public static void DeleteEntity(int entityIndex)
    {
        int temp0 = entityIndex;
        API.DeleteEntity(ref temp0);
    }

    public static void DeleteObject(ref int objectIndex)
        => API.DeleteObject(ref objectIndex);

    public static void DeleteObject(int objectIndex)
    {
        int temp0 = objectIndex;
        API.DeleteObject(ref temp0);
    }

    public static void DeleteVehicle(ref int vehicleIndex)
        => API.DeleteVehicle(ref vehicleIndex);

    public static void DeleteVehicle(int vehicleIndex)
    {
        int temp0 = vehicleIndex;
        API.DeleteVehicle(ref temp0);
    }

    public static void DisplayRadar(bool bDisplayRadarFlag)
        => API.DisplayRadar(bDisplayRadarFlag);

    public static bool DoorSystemFindExistingDoor(float position_x, float position_y, float position_z, uint model, out int doorEnumHash)
    {
        int temp4 = default;
        var __result = API.DoorSystemFindExistingDoor(position_x, position_y, position_z, unchecked((int)model), ref temp4);
        doorEnumHash = temp4;
        return __result;
    }

    public static void DrawMarkerSphere(float scrVecPosition_x, float scrVecPosition_y, float scrVecPosition_z, float radius, int colR, int colG, int colB, float intensity)
        => Function.Call((Hash)0x799017F9E3B10112UL, scrVecPosition_x, scrVecPosition_y, scrVecPosition_z, radius, colR, colG, colB, intensity);

    public static void DrawMarker_2(int markerType, float scrVecPosition_x, float scrVecPosition_y, float scrVecPosition_z, float scrVecDirection_x, float scrVecDirection_y, float scrVecDirection_z, float scrVecRotation_x, float scrVecRotation_y, float scrVecRotation_z, float scrVecScale_x, float scrVecScale_y, float scrVecScale_z, int colR, int colG, int colB, int colA, bool bounce, bool faceCam, int rotOrder, bool rotate, string txdName, string texName, bool invert, bool usePreAlphaDepth, bool matchEntityRotOrder)
        => Function.Call((Hash)0xE82728F0DE75D13AUL, markerType, scrVecPosition_x, scrVecPosition_y, scrVecPosition_z, scrVecDirection_x, scrVecDirection_y, scrVecDirection_z, scrVecRotation_x, scrVecRotation_y, scrVecRotation_z, scrVecScale_x, scrVecScale_y, scrVecScale_z, colR, colG, colB, colA, bounce, faceCam, rotOrder, rotate, txdName, texName, invert, usePreAlphaDepth, matchEntityRotOrder);

    public static void DrawRect(float centreX, float centreY, float width, float height, int r, int g, int b, int a, bool bStereo)
        => Function.Call((Hash)0x3A618A217E5154F0UL, centreX, centreY, width, height, r, g, b, a, bStereo);

    public static void EndTextCommandDisplayText(float displayAtX, float displayAtY, int sTEREO)
        => Function.Call((Hash)0xCD015E5BB0D96A57UL, displayAtX, displayAtY, sTEREO);

    public static bool FindSpawnPointInDirection(float vPosition_x, float vPosition_y, float vPosition_z, float vDirection_x, float vDirection_y, float vDirection_z, float fIdealSpawnDistance, out Vector3 vSpawnPoint)
    {
        Vector3 temp7 = default;
        var __result = API.FindSpawnPointInDirection(vPosition_x, vPosition_y, vPosition_z, vDirection_x, vDirection_y, vDirection_z, fIdealSpawnDistance, ref temp7);
        vSpawnPoint = temp7;
        return __result;
    }

    public static void FootVfxSetOverrideIceWithSnow(bool enable)
        => Function.Call((Hash)0xA342A3763B3AFB6CUL, enable);

    public static void ForceGlobalSnowFx(bool enable)
        => Function.Call((Hash)0x6E9EF3A33C8899F8UL, enable);

    public static bool GetAmmoInClip(int pedIndex, uint weaponType, out int returnAmmoInClip)
    {
        int temp2 = default;
        var __result = API.GetAmmoInClip(pedIndex, weaponType, ref temp2);
        returnAmmoInClip = temp2;
        return __result;
    }

    public static bool GetClosestVehicleNodeWithHeading(float vecCoors_x, float vecCoors_y, float vecCoors_z, out Vector3 vecReturnNearestNodeCoors, out float returnHeading, int nodeFlags, float zMeasureMult, float zTolerance)
    {
        var output3 = new OutputArgument();
        var output4 = new OutputArgument();
        var __result = Function.Call<int>((Hash)0xFF071FB798B803B0UL, vecCoors_x, vecCoors_y, vecCoors_z, output3, output4, nodeFlags, zMeasureMult, zTolerance) != 0;
        vecReturnNearestNodeCoors = output3.GetResult<Vector3>();
        returnHeading = output4.GetResult<float>();
        return __result;
    }

    public static string GetControlInstructionalButton(int control, int action, bool allowXOSwap)
        => Function.Call<string>((Hash)0x0499D7B09FC9B407UL, control, action, allowXOSwap);

    public static uint GetEntityModel(int entityIndex)
        => unchecked((uint)API.GetEntityModel(entityIndex));

    public static bool GetEntityProofs(int entityIndex, out bool bulletProofFlag, out bool flameProofFlag, out bool explosionProofFlag, out bool collisionProofFlag, out bool meleeProofFlag, out bool steamProofFlag, out bool dontResetDamageFlagsOnCleanupMissionState, out bool smokeProofFlag)
    {
        bool temp1 = default;
        bool temp2 = default;
        bool temp3 = default;
        bool temp4 = default;
        bool temp5 = default;
        bool temp6 = default;
        bool temp7 = default;
        bool temp8 = default;
        var __result = API.GetEntityProofs(entityIndex, ref temp1, ref temp2, ref temp3, ref temp4, ref temp5, ref temp6, ref temp7, ref temp8);
        bulletProofFlag = temp1;
        flameProofFlag = temp2;
        explosionProofFlag = temp3;
        collisionProofFlag = temp4;
        meleeProofFlag = temp5;
        steamProofFlag = temp6;
        dontResetDamageFlagsOnCleanupMissionState = temp7;
        smokeProofFlag = temp8;
        return __result;
    }

    public static void GetForcedComponent(uint nameHash, int forcedComponentIndex, out int forcedComponentNameHash, out int forcedComponentEnumValue, out int forcedComponentType)
    {
        int temp2 = default;
        int temp3 = default;
        int temp4 = default;
        API.GetForcedComponent(nameHash, forcedComponentIndex, ref temp2, ref temp3, ref temp4);
        forcedComponentNameHash = temp2;
        forcedComponentEnumValue = temp3;
        forcedComponentType = temp4;
    }

    public static bool GetGroundZFor_3dCoord(float vecCoors_x, float vecCoors_y, float vecCoors_z, out float returnZ, bool waterAsGround, bool ignoreDistToWaterLevelCheck)
    {
        var output3 = new OutputArgument();
        var __result = Function.Call<int>((Hash)0xC906A7DAB05C8D2BUL, vecCoors_x, vecCoors_y, vecCoors_z, output3, waterAsGround, ignoreDistToWaterLevelCheck) != 0;
        returnZ = output3.GetResult<float>();
        return __result;
    }

    public static void GetHudColour(int hudColour, out int red, out int green, out int blue, out int alpha_param)
    {
        int temp1 = default;
        int temp2 = default;
        int temp3 = default;
        int temp4 = default;
        API.GetHudColour(hudColour, ref temp1, ref temp2, ref temp3, ref temp4);
        red = temp1;
        green = temp2;
        blue = temp3;
        alpha_param = temp4;
    }

    public static bool GetMaxAmmo(int pedIndex, uint weaponType, out int returnMaxAmmo)
    {
        int temp2 = default;
        var __result = API.GetMaxAmmo(pedIndex, weaponType, ref temp2);
        returnMaxAmmo = temp2;
        return __result;
    }

    public static void GetModelDimensions(uint modelHashKey, out Vector3 returnMin, out Vector3 returnMax)
    {
        Vector3 temp1 = default;
        Vector3 temp2 = default;
        API.GetModelDimensions(modelHashKey, ref temp1, ref temp2);
        returnMin = temp1;
        returnMax = temp2;
    }

    public static bool GetNthClosestVehicleNode(float vecCoors_x, float vecCoors_y, float vecCoors_z, int nodeNumber, out Vector3 vecReturnNearestNodeCoors, int nodeFlags, float zMeasureMult, float zTolerance)
    {
        var output4 = new OutputArgument();
        var __result = Function.Call<int>((Hash)0xE50E52416CCF948BUL, vecCoors_x, vecCoors_y, vecCoors_z, nodeNumber, output4, nodeFlags, zMeasureMult, zTolerance) != 0;
        vecReturnNearestNodeCoors = output4.GetResult<Vector3>();
        return __result;
    }

    public static bool GetNthClosestVehicleNodeWithHeading(float vecCoors_x, float vecCoors_y, float vecCoors_z, int nodeNumber, out Vector3 vecReturnNearestNodeCoors, out float returnHeading, out int returnNumLanes, int nodeFlags, float zMeasureMult, float zTolerance)
    {
        Vector3 temp4 = default;
        float temp5 = default;
        int temp6 = default;
        var __result = API.GetNthClosestVehicleNodeWithHeading(vecCoors_x, vecCoors_y, vecCoors_z, nodeNumber, ref temp4, ref temp5, ref temp6, nodeFlags, zMeasureMult, zTolerance);
        vecReturnNearestNodeCoors = temp4;
        returnHeading = temp5;
        returnNumLanes = temp6;
        return __result;
    }

    public static uint GetPedAmmoTypeFromWeapon(int pedIndex, uint weaponType)
        => unchecked((uint)API.GetPedAmmoTypeFromWeapon(pedIndex, weaponType));

    public static uint GetPedCauseOfDeath(int pedIndex)
        => unchecked((uint)API.GetPedCauseOfDeath(pedIndex));

    public static bool GetPedCurrentMoveBlendRatio(int pedIndex, out float mbrXOut, out float mbrYOut)
    {
        var output1 = new OutputArgument();
        var output2 = new OutputArgument();
        var __result = Function.Call<int>((Hash)0xF60165E1D2C5370BUL, pedIndex, output1, output2) != 0;
        mbrXOut = output1.GetResult<float>();
        mbrYOut = output2.GetResult<float>();
        return __result;
    }

    public static bool GetPedHeadOverlayData(int p0, int p1, out int p2, out int p3, out int p4, out int p5, out float p6)
    {
        int temp2 = default;
        int temp3 = default;
        int temp4 = default;
        int temp5 = default;
        float temp6 = default;
        var __result = API.GetPedHeadOverlayData(p0, p1, ref temp2, ref temp3, ref temp4, ref temp5, ref temp6);
        p2 = temp2;
        p3 = temp3;
        p4 = temp4;
        p5 = temp5;
        p6 = temp6;
        return __result;
    }

    public static int GetPedInVehicleSeat(int vehicleIndex, int seat, bool considerPedUsingSeat)
        => Function.Call<int>((Hash)0xBB40DD2270B65366UL, vehicleIndex, seat, considerPedUsingSeat);

    public static int GetPedPropIndex(int pedIndex, int position, bool doDeadCheck)
        => Function.Call<int>((Hash)0x898CC20EA75BACD8UL, pedIndex, position, doDeadCheck);

    public static bool GetPointOnRoadSide(float vInputNodePosition_x, float vInputNodePosition_y, float vInputNodePosition_z, int iDirection, out Vector3 vOutPositionByRoad)
    {
        Vector3 temp4 = default;
        var __result = API.GetPointOnRoadSide(vInputNodePosition_x, vInputNodePosition_y, vInputNodePosition_z, iDirection, ref temp4);
        vOutPositionByRoad = temp4;
        return __result;
    }

    public static bool GetSafeCoordForPed(float vecCoors_x, float vecCoors_y, float vecCoors_z, bool bOnlyOnPavement, out Vector3 vecReturnSafeCoors, int iFlags)
    {
        Vector3 temp4 = default;
        var __result = API.GetSafeCoordForPed(vecCoors_x, vecCoors_y, vecCoors_z, bOnlyOnPavement, ref temp4, iFlags);
        vecReturnSafeCoors = temp4;
        return __result;
    }

    public static uint GetSelectedPedWeapon(int pedIndex)
        => unchecked((uint)API.GetSelectedPedWeapon(pedIndex));

    public static int GetShapeTestResult(int shapeTestGuid, out int bHitSomething, out Vector3 vPos, out Vector3 vNormal, out int entityIndex)
    {
        var output1 = new OutputArgument();
        var output2 = new OutputArgument();
        var output3 = new OutputArgument();
        var output4 = new OutputArgument();
        var __result = Function.Call<int>((Hash)0x3D87450E15D98694UL, shapeTestGuid, output1, output2, output3, output4);
        bHitSomething = output1.GetResult<int>();
        vPos = output2.GetResult<Vector3>();
        vNormal = output3.GetResult<Vector3>();
        entityIndex = output4.GetResult<int>();
        return __result;
    }

    public static float GetSnowLevel()
        => Function.Call<float>((Hash)0xC5868A966E5BE3AEUL);

    public static void GetStreetNameAtCoord(float searchCoords_x, float searchCoords_y, float searchCoords_z, out int hashName1, out int hashName2)
    {
        uint temp3 = default;
        uint temp4 = default;
        API.GetStreetNameAtCoord(searchCoords_x, searchCoords_y, searchCoords_z, ref temp3, ref temp4);
        hashName1 = unchecked((int)temp3);
        hashName2 = unchecked((int)temp4);
    }

    public static void GetUtcTime(out int year, out int month, out int day, out int hour, out int min, out int sec)
    {
        int temp0 = default;
        int temp1 = default;
        int temp2 = default;
        int temp3 = default;
        int temp4 = default;
        int temp5 = default;
        API.GetUtcTime(ref temp0, ref temp1, ref temp2, ref temp3, ref temp4, ref temp5);
        year = temp0;
        month = temp1;
        day = temp2;
        hour = temp3;
        min = temp4;
        sec = temp5;
    }

    public static void GetVariantComponent(uint nameHash, int variantComponentIndex, out int variantComponentNameHash, out int variantComponentEnumValue, out int variantComponentType)
    {
        int temp2 = default;
        int temp3 = default;
        int temp4 = default;
        API.GetVariantComponent(nameHash, variantComponentIndex, ref temp2, ref temp3, ref temp4);
        variantComponentNameHash = temp2;
        variantComponentEnumValue = temp3;
        variantComponentType = temp4;
    }

    public static void GetVariantProp(uint nameHash, int variantPropIndex, out int variantPropNameHash, out int variantPropEnumValue, out int variantPropAnchor)
    {
        var output2 = new OutputArgument();
        var output3 = new OutputArgument();
        var output4 = new OutputArgument();
        Function.Call((Hash)0xD81B7F27BC773E66UL, nameHash, variantPropIndex, output2, output3, output4);
        variantPropNameHash = output2.GetResult<int>();
        variantPropEnumValue = output3.GetResult<int>();
        variantPropAnchor = output4.GetResult<int>();
    }

    public static void GetVehicleColours(int vehicleIndex, out int returnColour1, out int returnColour2)
    {
        int temp1 = default;
        int temp2 = default;
        API.GetVehicleColours(vehicleIndex, ref temp1, ref temp2);
        returnColour1 = temp1;
        returnColour2 = temp2;
    }

    public static void GetVehicleCustomPrimaryColour(int vehicleIndex, out int red, out int green, out int blue)
    {
        int temp1 = default;
        int temp2 = default;
        int temp3 = default;
        API.GetVehicleCustomPrimaryColour(vehicleIndex, ref temp1, ref temp2, ref temp3);
        red = temp1;
        green = temp2;
        blue = temp3;
    }

    public static void GetVehicleCustomSecondaryColour(int vehicleIndex, out int red, out int green, out int blue)
    {
        int temp1 = default;
        int temp2 = default;
        int temp3 = default;
        API.GetVehicleCustomSecondaryColour(vehicleIndex, ref temp1, ref temp2, ref temp3);
        red = temp1;
        green = temp2;
        blue = temp3;
    }

    public static void GetVehicleDashboardColour(int vehicleIndex, out int returnExtraCarColour6)
    {
        int temp1 = default;
        API.GetVehicleDashboardColour(vehicleIndex, ref temp1);
        returnExtraCarColour6 = temp1;
    }

    public static void GetVehicleExtraColours(int vehicleIndex, out int returnExtraCarColour1, out int returnExtraCarColour2)
    {
        int temp1 = default;
        int temp2 = default;
        API.GetVehicleExtraColours(vehicleIndex, ref temp1, ref temp2);
        returnExtraCarColour1 = temp1;
        returnExtraCarColour2 = temp2;
    }

    public static void GetVehicleInteriorColour(int vehicleIndex, out int returnExtraCarColour5)
    {
        int temp1 = default;
        API.GetVehicleInteriorColour(vehicleIndex, ref temp1);
        returnExtraCarColour5 = temp1;
    }

    public static void GetVehicleModColor_1(int vehicle, out int colorType, out int baseColIndex, out int specColIndex)
    {
        int temp1 = default;
        int temp2 = default;
        int temp3 = default;
        API.GetVehicleModColor_1(vehicle, ref temp1, ref temp2, ref temp3);
        colorType = temp1;
        baseColIndex = temp2;
        specColIndex = temp3;
    }

    public static void GetVehicleModColor_2(int vehicle, out int colorType, out int baseColIndex)
    {
        int temp1 = default;
        int temp2 = default;
        API.GetVehicleModColor_2(vehicle, ref temp1, ref temp2);
        colorType = temp1;
        baseColIndex = temp2;
    }

    public static int GetVehicleModVariation(int vehicle, int modSlot)
        => Function.Call<int>((Hash)0xB3924ECD70E095DCUL, vehicle, modSlot);

    public static void GetVehicleNeonLightsColour(int vehicleIndex, out int red, out int green, out int blue)
    {
        int temp1 = default;
        int temp2 = default;
        int temp3 = default;
        API.GetVehicleNeonLightsColour(vehicleIndex, ref temp1, ref temp2, ref temp3);
        red = temp1;
        green = temp2;
        blue = temp3;
    }

    public static void GetVehicleTyreSmokeColor(int vehicle, out int red, out int green, out int blue)
    {
        int temp1 = default;
        int temp2 = default;
        int temp3 = default;
        API.GetVehicleTyreSmokeColor(vehicle, ref temp1, ref temp2, ref temp3);
        red = temp1;
        green = temp2;
        blue = temp3;
    }

    public static bool GetVehicleXenonLightsCustomColor(int p0, out int p1, out int p2, out int p3)
    {
        int temp1 = default;
        int temp2 = default;
        int temp3 = default;
        var __result = API.GetVehicleXenonLightsCustomColor(p0, ref temp1, ref temp2, ref temp3);
        p1 = temp1;
        p2 = temp2;
        p3 = temp3;
        return __result;
    }

    public static void GetWeatherTypeTransition(out int prevTypeHashName, out int nextTypeHashName, out float interpVal)
    {
        uint temp0 = default;
        uint temp1 = default;
        float temp2 = default;
        API.GetWeatherTypeTransition(ref temp0, ref temp1, ref temp2);
        prevTypeHashName = unchecked((int)temp0);
        nextTypeHashName = unchecked((int)temp1);
        interpVal = temp2;
    }

    public static bool HasPedGotWeapon(int pedIndex, uint typeOfWeapon, int generalWeaponType)
        => Function.Call<int>((Hash)0x8DECB02F88F428BCUL, pedIndex, typeOfWeapon, generalWeaponType) != 0;

    public static bool IsContentItemLocked(uint lockHash)
    {
        int temp0 = unchecked((int)lockHash);
        return API.IsContentItemLocked(ref temp0);
    }

    public static bool IsEntityDead(int entityIndex, bool bIgnoreVehicleDrowningIfInvincible)
        => Function.Call<int>((Hash)0x5F9532F3B5CC2551UL, entityIndex, bIgnoreVehicleDrowningIfInvincible) != 0;

    public static bool IsPedBeingStunned(int pedIndex, uint weaponType)
        => API.IsPedBeingStunned(pedIndex, unchecked((int)weaponType));

    public static bool IsPedDoingABeastJump(int pedIndex)
        => Function.Call<int>((Hash)0x451D05012CCEC234UL, pedIndex) != 0;

    public static bool IsPedLanding(int pedIndex)
        => Function.Call<int>((Hash)0x412F1364FA066CFBUL, pedIndex) != 0;

    public static bool IsThisModelAnAmphibiousQuadbike(uint vehicleModelHashKey)
        => API.IsThisModelAnAmphibiousQuadbike(unchecked((int)vehicleModelHashKey));

    public static bool IsUsingKeyboardAndMouse(int control)
        => Function.Call<int>((Hash)0xA571D46727E2B718UL, control) != 0;

    public static bool IsVehicleSeatFree(int vehicleIndex, int seat, bool considerPedUsingSeat)
        => Function.Call<int>((Hash)0x22AC59A870E6A669UL, vehicleIndex, seat, considerPedUsingSeat) != 0;

    public static bool NetworkExplodeVehicle(int vehicleIndex, bool addExplosion, bool keepDamageEntity, int culprit)
        => Function.Call<int>((Hash)0x301A42153C9AD707UL, vehicleIndex, addExplosion, keepDamageEntity, culprit) != 0;

    public static void NetworkFadeInEntity(int entityIndex, bool bNetwork, bool flash)
        => Function.Call((Hash)0x1F4ED342ACEFE62DUL, entityIndex, bNetwork, flash);

    public static void RemoveBlip(ref int blipId)
        => API.RemoveBlip(ref blipId);

    public static void RemoveBlip(int blipId)
    {
        int temp0 = blipId;
        API.RemoveBlip(ref temp0);
    }

    public static void RemoveDoorFromSystem(uint doorEnumHash, bool bLock)
        => Function.Call((Hash)0x464D8E1427156FE4UL, doorEnumHash, bLock);

    public static void RemoveModelHide(float pos_x, float pos_y, float pos_z, float radius, uint modelHash, bool bLazy)
        => Function.Call((Hash)0xD9E3006FB3CBD765UL, pos_x, pos_y, pos_z, radius, modelHash, bLazy);

    public static void RenderScriptCams(bool bSetActive, bool doGameCamInterp, int duration, bool bShouldLockInterpolationSourceFrame, bool bShouldApplyAcrossAllThreads, int renderingOptions)
        => Function.Call((Hash)0x07E5B515DB0636FCUL, bSetActive, doGameCamInterp, duration, bShouldLockInterpolationSourceFrame, bShouldApplyAcrossAllThreads, renderingOptions);

    public static void RequestCollisionAtCoord(float vecCoors_x, float vecCoors_y, float vecCoors_z)
        => API.RequestCollisionAtCoord(vecCoors_x, vecCoors_y, vecCoors_z);

    public static bool RequestScriptAudioBank(string bankName, bool bOverNetwork, int playerBits)
        => Function.Call<int>((Hash)0x2F844A8B08D76685UL, bankName, bOverNetwork, playerBits) != 0;

    public static void ResetEntityAlpha(int entityIndex)
        => API.ResetEntityAlpha(entityIndex);

    public static void ResetPedVisibleDamage(int pedIndex)
        => API.ResetPedVisibleDamage(pedIndex);

    public static void RollDownWindow(int vehicleIndex, int windowNumber)
        => API.RollDownWindow(vehicleIndex, windowNumber);

    public static void RollUpWindow(int vehicleIndex, int windowNumber)
        => API.RollUpWindow(vehicleIndex, windowNumber);

    public static void SetAllMpGamerTagsVisibility(int iPlayerNum, bool bVisible)
        => Function.Call((Hash)0xEE76FF7E6A0166B0UL, iPlayerNum, bVisible);

    public static void SetBeastJumpThisFrame(int pedIndex)
        => Function.Call((Hash)0x438822C279B73B93UL, pedIndex);

    public static void SetBlipSecondaryColour(int blip, int r, int g, int b)
        => Function.Call((Hash)0x14892474891E09EBUL, blip, r, g, b);

    public static void SetCutsceneEntityStreamingFlags(string sceneHandle, uint model, int streamingFlags)
        => API.SetCutsceneEntityStreamingFlags(sceneHandle, unchecked((int)model), streamingFlags);

    public static void SetDrawOrigin(float vecOrigin_x, float vecOrigin_y, float vecOrigin_z, bool bIs2d)
        => Function.Call((Hash)0xAA0008F3BBB8F416UL, vecOrigin_x, vecOrigin_y, vecOrigin_z, bIs2d);

    public static void SetEntityAlpha(int entityIndex, int alpha_param, bool bUseSmoothAlpha)
        => Function.Call((Hash)0x44A0870B7E92D7C0UL, entityIndex, alpha_param, bUseSmoothAlpha);

    public static void SetEntityAsNoLongerNeeded(ref int entityIndex)
        => API.SetEntityAsNoLongerNeeded(ref entityIndex);

    public static void SetEntityAsNoLongerNeeded(int entityIndex)
    {
        int temp0 = entityIndex;
        API.SetEntityAsNoLongerNeeded(ref temp0);
    }

    public static void SetEntityHealth(int entityIndex, int newHealth, int instigatorIndex, int weaponHashForTelemetry)
        => Function.Call((Hash)0x6B76DC1F3AE6E6A3UL, entityIndex, newHealth, instigatorIndex, weaponHashForTelemetry);

    public static void SetEntityInvincible(int entityIndex, bool invincible, bool dontResetDamageFlagsOnCleanupMissionState)
        => Function.Call((Hash)0x3882114BDE571AD4UL, entityIndex, invincible, dontResetDamageFlagsOnCleanupMissionState);

    public static void SetForcedJumpThisFrame(int pedIndex)
        => Function.Call((Hash)0xA1183BCFEE0F93D1UL, pedIndex);

    public static void SetHornPermanentlyOnTime(int vehicleIndex, float hornTime)
        => Function.Call((Hash)0x9D3AF56E94C9AE98UL, vehicleIndex, hornTime);

    public static void SetInvertVehicleControls(int vehicleIndex, bool invertControls)
        => Function.Call((Hash)0x5B91B229243351A8UL, vehicleIndex, invertControls);

    public static void SetMpGamerTagVisibility(int iPlayerNum, int iTag, bool bVisible, bool bEvenInCars)
        => Function.Call((Hash)0x63BB75ABEDC1F6A0UL, iPlayerNum, iTag, bVisible, bEvenInCars);

    public static void SetPedAmmo(int pedIndex, uint typeOfWeapon, int amountOfAmmo, bool ignoreDeadCheck)
        => Function.Call((Hash)0x14E56BC5B5DB6A19UL, pedIndex, typeOfWeapon, amountOfAmmo, ignoreDeadCheck);

    public static void SetPedAsNoLongerNeeded(ref int pedIndex)
        => API.SetPedAsNoLongerNeeded(ref pedIndex);

    public static void SetPedAsNoLongerNeeded(int pedIndex)
    {
        int temp0 = pedIndex;
        API.SetPedAsNoLongerNeeded(ref temp0);
    }

    public static void SetPedMoveRateInWaterOverride(int pedIndex, float desiredRate)
        => Function.Call((Hash)0x0B3E35AC043707D9UL, pedIndex, desiredRate);

    public static void SetPedPropIndex(int pedIndex, int position, int newPropIndex, int newTextIndex, bool syncWithBlend, bool doDeadCheck)
        => Function.Call((Hash)0x93376B65A266EB5FUL, pedIndex, position, newPropIndex, newTextIndex, syncWithBlend, doDeadCheck);

    public static void SetPedRandomComponentVariation(int pedIndex, int race)
        => Function.Call((Hash)0xC8A9481A01E63C28UL, pedIndex, race);

    public static void SetScaleformMovieAsNoLongerNeeded(ref int iScaleformMovieId)
        => API.SetScaleformMovieAsNoLongerNeeded(ref iScaleformMovieId);

    public static void SetScaleformMovieAsNoLongerNeeded(int iScaleformMovieId)
    {
        int temp0 = iScaleformMovieId;
        API.SetScaleformMovieAsNoLongerNeeded(ref temp0);
    }

    public static void SetSuperJumpThisFrame(int pedIndex)
        => API.SetSuperJumpThisFrame(pedIndex);

    public static void SetVehicleAsNoLongerNeeded(ref int vehicleIndex)
        => API.SetVehicleAsNoLongerNeeded(ref vehicleIndex);

    public static void SetVehicleAsNoLongerNeeded(int vehicleIndex)
    {
        int temp0 = vehicleIndex;
        API.SetVehicleAsNoLongerNeeded(ref temp0);
    }

    public static bool SetVehicleOnGroundProperly(int vehicleIndex, float hightSampleRangeUp)
        => Function.Call<int>((Hash)0x49733E92263139D1UL, vehicleIndex, hightSampleRangeUp) != 0;

    public static void SetVehicleReduceGripLevel(int vehicleIndex, int reducedGripLevel)
        => Function.Call((Hash)0x6DEE944E1EE90CFBUL, vehicleIndex, reducedGripLevel);

    public static void SetVehicleWheelsCanBreak(int vehicleIndex, bool canBreakWheelsFlag)
        => API.SetVehicleWheelsCanBreak(vehicleIndex, canBreakWheelsFlag);

    public static int SetupShopPedApparelQueryTu(int character, int shop, int locate, int apparelType, int anchorPoint, int componentType)
        => Function.Call<int>((Hash)0x9BDF59818B1E38C1UL, character, shop, locate, apparelType, anchorPoint, componentType);

    public static void SmashVehicleWindow(int vehicleIndex, int windowNumber)
        => API.SmashVehicleWindow(vehicleIndex, windowNumber);

    public static int StartNetworkedParticleFxLoopedOnEntity(string fxName, int entityIndex, float vecPosition_x, float vecPosition_y, float vecPosition_z, float vecRotation_x, float vecRotation_y, float vecRotation_z, float scale, bool invertAxisX, bool invertAxisY, bool invertAxisZ, float colorR, float colorG, float colorB, bool terminateOnOwnerLeave)
        => Function.Call<int>((Hash)0x6F60E89A7B64EE1DUL, fxName, entityIndex, vecPosition_x, vecPosition_y, vecPosition_z, vecRotation_x, vecRotation_y, vecRotation_z, scale, invertAxisX, invertAxisY, invertAxisZ, colorR, colorG, colorB, terminateOnOwnerLeave);

    public static bool StartParticleFxNonLoopedAtCoord(string fxName, float vecPosition_x, float vecPosition_y, float vecPosition_z, float vecRotation_x, float vecRotation_y, float vecRotation_z, float scale, bool invertAxisX, bool invertAxisY, bool invertAxisZ)
        => Function.Call<int>((Hash)0x25129531F77B9ED3UL, fxName, vecPosition_x, vecPosition_y, vecPosition_z, vecRotation_x, vecRotation_y, vecRotation_z, scale, invertAxisX, invertAxisY, invertAxisZ) != 0;

    public static void StartVehicleAlarm(int vehicleIndex)
        => API.StartVehicleAlarm(vehicleIndex);

    public static void TaskEnterVehicle(int pedIndex, int vehicleIndex, int time, int seat, float moveBlendRatio, int iFlags, string overridenClipSet)
        => Function.Call((Hash)0xC20E50AA46D09CA8UL, pedIndex, vehicleIndex, time, seat, moveBlendRatio, iFlags, overridenClipSet);

    public static void TaskHeliMission(int pedIndex, int heliIndex, int targetVehicleIndex, int targetPedIndex, float vecCoors_x, float vecCoors_y, float vecCoors_z, int mission, float cruiseSpeed, float targetReachedDist, float heliOrientation, int flightHeight, int minHeightAboveTerrain, float fSlowDownDistance, int heliFlags)
        => Function.Call((Hash)0xDAD029E187A2BEB4UL, pedIndex, heliIndex, targetVehicleIndex, targetPedIndex, vecCoors_x, vecCoors_y, vecCoors_z, mission, cruiseSpeed, targetReachedDist, heliOrientation, flightHeight, minHeightAboveTerrain, fSlowDownDistance, heliFlags);

    public static void TaskLookAtCoord(int pedIndex, float vecCoors_x, float vecCoors_y, float vecCoors_z, int time, int lookFlags, int priority)
        => Function.Call((Hash)0x6FA46612594F7973UL, pedIndex, vecCoors_x, vecCoors_y, vecCoors_z, time, lookFlags, priority);

    public static void TaskPlaneMission(int pedIndex, int planeIndex, int targetVehicleIndex, int targetPedIndex, float vecCoors_x, float vecCoors_y, float vecCoors_z, int mission, float cruiseSpeed, float targetReachedDist, float planeOrientation, int flightHeight, int minHeightAboveTerrain, bool bPrecise)
        => Function.Call((Hash)0x23703CD154E83B88UL, pedIndex, planeIndex, targetVehicleIndex, targetPedIndex, vecCoors_x, vecCoors_y, vecCoors_z, mission, cruiseSpeed, targetReachedDist, planeOrientation, flightHeight, minHeightAboveTerrain, bPrecise);

    public static void TaskPlayAnim(int pedIndex, string pAnimDictName, string pAnimName, float fBlendInDelta, float fBlendOutDelta, int nTimeToPlay, int animFlags, float startPhase, bool phaseControlled, int ikFlags, bool bAllowOverrideCloneUpdate)
        => Function.Call((Hash)0xEA47FE3719165B94UL, pedIndex, pAnimDictName, pAnimName, fBlendInDelta, fBlendOutDelta, nTimeToPlay, animFlags, startPhase, phaseControlled, ikFlags, bAllowOverrideCloneUpdate);

    public static void UseSnowFootVfxWhenUnsheltered(bool enable)
        => Function.Call((Hash)0xAEEDAD1420C65CC0UL, enable);

    public static void UseSnowWheelVfxWhenUnsheltered(bool enable)
        => Function.Call((Hash)0x4CC7F0FEA5283FE0UL, enable);
}
