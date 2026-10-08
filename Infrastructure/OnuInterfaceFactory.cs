using RLab.Abstractions;

namespace RLab.Infrastructure;
public class OnuInterfaceFactory : IOnuInterfaceFactory
{
    private readonly IHuaweiBlue _huaweiBlue;
    private readonly IHuaweiRed _huaweiRed;
    private readonly INokia _nokia;
    private readonly INbel _nbel;

    public OnuInterfaceFactory(IHuaweiBlue huaweiBlue, IHuaweiRed huaweiRed, INokia nokia, INbel nbel)
    {
        _huaweiBlue = huaweiBlue;
        _huaweiRed = huaweiRed;
        _nokia = nokia;
        _nbel = nbel;
    }

    public IOnu Create(string model)
    {
        if (IsHuaweiBlue(model))
            return _huaweiBlue;

        if (IsHuaweiRed(model))
            return _huaweiRed;

        if (IsNokia(model))
            return _nokia;

        if (IsNbel(model))
            return _nbel;

        throw new NotSupportedException(
            $"O modelo '{model}' não é suportado.");
    }

    private static bool IsHuaweiBlue(string model)
        => OnuModels.HuaweiBlue.Contains(model);

    private static bool IsHuaweiRed(string model)
        => OnuModels.HuaweiRed.Contains(model);

    private static bool IsNokia(string model)
        => OnuModels.Nokia.Contains(model);

    private static bool IsNbel(string model)
        => OnuModels.Nbel.Contains(model);
}
