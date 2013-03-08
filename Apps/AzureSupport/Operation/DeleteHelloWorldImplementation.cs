namespace TheBall.Demo
{
    public class DeleteHelloWorldImplementation
    {
        public static HelloWorldObject GetTarget_ObjectToDelete(string id)
        {
            var result = HelloWorldObject.RetrieveFromOwnerContent(InformationContext.Current.CurrentOwner, id);
            return result;
        }

        public static void ExecuteMethod_DeleteObject(HelloWorldObject objectToDelete)
        {
            objectToDelete.DeleteInformationObject();
        }
    }
}