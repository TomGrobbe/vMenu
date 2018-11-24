function Export_GetPedHeadBlendData() {
    var arr = new Uint32Array(new ArrayBuffer(10 * 8)); // int, int, int, int, int, int, float, float, float, bool
    Citizen.invokeNative("0x2746BD9D88C5C5D0", GetPlayerPed(-1), arr);
    return JSON.stringify(arr);
}

on('GetPedHeadBlendData', (cb) => {
    cb(Export_GetPedHeadBlendData());
});
