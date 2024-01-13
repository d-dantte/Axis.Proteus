namespace Axis.Proteus.SimpleInjector.Test.Types
{

    #region Test Types
    public interface I1 { }

	public interface I2 { }

	public interface I3 { }


	public interface I1A : I1 { }

	public interface I2A : I2 { }

	public interface I1I2A : I1, I2 { }

	public interface I1I2I3A : I1, I2, I3 { }


	public class C_I1 : I1 { }

	public class C_I2 : I2 { }

	public class C_I3 : I3 { }

	public class C_I1_I2 : I1, I2 { }

	public class C_I1I2A : I1I2A { }

	public class C_I1I2I3A : I1I2I3A { }

	public class C_I1_I1A : I1, I1A { }

	public class C_I1_2 : I1
	{
		public TheClass Property { get; }

		public C_I1_2(TheClass arg) => Property = arg;
	}
	#endregion

	public class OtherClass
	{
		public I2 Service { get; set; }

		public OtherClass(I2 service)
		{
			Service = service;
		}
	}

	public class TheClass
	{
	}
}
