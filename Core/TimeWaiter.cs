
namespace GitAutoUpdater.Core
{
    public class TimeWaiter
    {
        private readonly string[] frames = { "/", "-", "\\" };
        private bool active = false;
        private int counter = 0;
        private Thread thread;

        /// <summary>
        /// Inicia el Timer
        /// </summary>
        public void Start(string text)
        {
            Logger.Log(text, Logger.LogLevel.Process, true);

            active = true;
            thread = new Thread(() =>
            {
                while (active)
                {
                    string frame = frames[counter % frames.Length];
                    Console.Write($"\r[{frame}] {text}");

                    counter++;
                    Thread.Sleep(250);
                }
            });

            thread.Start();
        }

        public void Stop(string msg = "", Logger.LogLevel type = Logger.LogLevel.Info)
        {

            active = false;
            thread.Join();

            Console.Write("\r"); // Cleans the line

            if (!string.IsNullOrWhiteSpace(msg))
            {
                Logger.Log(msg, type);
            }
        }
    }
}
