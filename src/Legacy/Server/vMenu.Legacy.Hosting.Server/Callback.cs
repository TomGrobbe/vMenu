using CitizenFX.Core;

namespace vMenu.Legacy.Hosting;

public sealed class Callback(CallbackDelegate callback)
{
    public void Invoke(params object[] args) => callback(args);

    public static Callback? From(IDictionary<string, object>? bag, string name) =>
        bag is not null && bag.TryGetValue(name, out var value) && value is CallbackDelegate callback ? new Callback(callback) : null;
}
