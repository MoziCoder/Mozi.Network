namespace Mozi.Telnet
{
    public interface ITelnetCommand
    {
        string Name { get; }
        string Descript();
        void Invoke(params string[] args);
    }



    public class Shell : ITelnetCommand
    {
        public string Name => this.GetType().Name;

        public string Descript()
        {
            return "";
        }

        public void Invoke(params string[] args)
        {
            
        }
    }
}
