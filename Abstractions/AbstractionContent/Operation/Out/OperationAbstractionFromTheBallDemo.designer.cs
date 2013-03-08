 

using System;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;

		namespace TheBall.Demo { 
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
				public class DeleteHelloWorldParameters 
		{
				public string ID ;
				}
		
		public class DeleteHelloWorld 
		{
				private static void PrepareParameters(DeleteHelloWorldParameters parameters)
		{
					}
				public static void Execute(DeleteHelloWorldParameters parameters)
		{
						PrepareParameters(parameters);
					HelloWorldObject ObjectToDelete = DeleteHelloWorldImplementation.GetTarget_ObjectToDelete(parameters.ID);	
				DeleteHelloWorldImplementation.ExecuteMethod_DeleteObject(ObjectToDelete);		
				}
				}
		} 