namespace Core.MVC
{
    public interface IController
    {
        public void SetModel(object model);
        public void SetView(object view);
    }
}