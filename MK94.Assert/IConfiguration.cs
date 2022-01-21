namespace MK94.Assert
{
    public interface IConfiguration
    {
        IConfiguration WithFolderStructure(BasedOn basedOn);

        IConfiguration WithChecksumStructure(BasedOn basedOn);
        
        IConfiguration WithPseudoRandom(BasedOn basedOn);
        
        IConfiguration WithDevModeOnEnvironmentVariable(string environmentVariable, string valueOnProd);
    }
}