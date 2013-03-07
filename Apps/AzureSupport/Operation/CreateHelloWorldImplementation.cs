using TheBall.CORE;

namespace TheBall.DEMO
{
    public class CreateHelloWorldImplementation
    {
        public static HelloWorldObject GetTarget_CreatedObject()
        {
            HelloWorldObject result = HelloWorldObject.CreateDefault();
            var owner = InformationContext.Current.CurrentOwner;
            result.SetLocationAsOwnerContent(owner, result.ID);
            return result;
        }

        public static void ExecuteMethod_SetHelloWorldText(string helloText, HelloWorldObject createdObject)
        {
            createdObject.HelloText = helloText;
        }

        public static void ExecuteMethod_StoreObject(HelloWorldObject createdObject)
        {
            createdObject.StoreInformationMasterFirst(InformationContext.Current.CurrentOwner, false);
        }
    }
}