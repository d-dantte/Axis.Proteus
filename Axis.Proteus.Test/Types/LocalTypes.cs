namespace Axis.Proteus.Test.Types
{
    #region I1
    public interface I1
    { }

    public class C_I1_1 : I1
    { }

    public class C_I1_2 : I1
    { }

    public class C_I1_3 : I1
    { }
    #endregion

    #region I2
    public interface I2 { }

    public class C_I2_1 : I2 { }

    public class C_I2_2 : I2 { }

    public class C_I2_3 : I2 { }
    #endregion

    public class C_I1_I2_1 : I1, I2 { }

    public class Class1
    {
        public I1 Service { get; set; }

        public Class1(I1 service)
        {
            Service = service;
        }
    }
}
