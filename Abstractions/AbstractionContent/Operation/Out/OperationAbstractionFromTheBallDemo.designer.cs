 

using System;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;

		namespace TheBall.DEMO { 
				public class CreateHelloWorldParameters 
		{
				public string HelloText ;
				}
		
		public class CreateHelloWorld 
		{
				private static void PrepareParameters(CreateHelloWorldParameters parameters)
		{
					}
				public static void Execute(CreateHelloWorldParameters parameters)
		{
						PrepareParameters(parameters);
					HelloWorldObject CreatedObject = CreateHelloWorldImplementation.GetTarget_CreatedObject();	
				CreateHelloWorldImplementation.ExecuteMethod_SetHelloWorldText(parameters.HelloText, CreatedObject);		
				CreateHelloWorldImplementation.ExecuteMethod_StoreObject(CreatedObject);		
				}
				}
		 } 