namespace Template.MobileApp.Usecase;

public class SampleUsecase
{
    private readonly NetworkOperator networkOperator;

    public SampleUsecase(
        NetworkOperator networkOperator)
    {
        this.networkOperator = networkOperator;
    }

    //--------------------------------------------------------------------------------
    // Simple
    //--------------------------------------------------------------------------------

    public ValueTask<IResult<ServerTimeResponse>> GetServerTimeAsync() =>
        networkOperator.ExecuteVerbose(n => n.GetServerTimeAsync());

    // TODO Error

    //--------------------------------------------------------------------------------
    // Data
    //--------------------------------------------------------------------------------

    // TODO

    //--------------------------------------------------------------------------------
    // Download/Upload
    //--------------------------------------------------------------------------------

    // TODO
}
