namespace RLab.Abstractions;
public interface IOnuInterfaceFactory
{
    IOnu Create(string model);
}