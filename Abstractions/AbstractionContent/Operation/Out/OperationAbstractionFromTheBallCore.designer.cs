 

using System;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;

		namespace TheBall.CORE { 
				public class CreateInvoiceForGroupParameters 
		{
				public string GroupID ;
				}
		
		public class CreateInvoiceForGroup 
		{
				private static void PrepareParameters(CreateInvoiceForGroupParameters parameters)
		{
					}
				public static void Execute(CreateInvoiceForGroupParameters parameters)
		{
						PrepareParameters(parameters);
					TBRGroupRoot GroupRoot = CreateInvoiceForGroupImplementation.GetTarget_GroupRoot(parameters.GroupID);	
				}
				}
		 } 