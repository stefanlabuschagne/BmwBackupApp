using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;
using WebApplication4.Models;
using System.Threading.Tasks;

namespace WebApplication1
{
    public class SimpleHub : Hub
    {

        private static string TheSourcePath;
        private static string TheTargetPath;

        // 
        // THIS IS SERVER SIDE SCRIPTS CALLED FROM THE CLIENT VIA SIGNALR!
        //

        // Client will invoke this Hello method through JS to get then update from the Server.
        public void hello()
        {
            // Server invokes the client method "UpdateProgress"
            Clients.All.updateProgress("Backup Log Detail");
        }

        //Client Calls this method to start the backup
        public void doBackup(string sourcePath, string targetPath)

        {

            // Lets check if the Paths exist!
            if (!System.IO.Directory.Exists(sourcePath)  )
                    {
                Clients.All.AlertUser("Invalid Source path specified.");
                return;
            }
            else
            {
                if (!System.IO.Directory.Exists(targetPath))
                {
                    Clients.All.AlertUser("Invalid Target path specified.");
                    return;
                }
                else
                {
                    SimpleHub.TheSourcePath = sourcePath.ToString();
                    SimpleHub.TheTargetPath = targetPath.ToString();
                }
            }
  
            // Clears the Progress feedback on the Client Screen.
            Clients.All.clearProgress();

            // Clients.All.updateProgress("Starting Backup. " + DateTime.Now.ToString() + sourcePath.ToString() + " to " + targetPath.ToString());

            String guid = Guid.NewGuid().ToString();

            // Updates the GUID on the Client Screen.
            Clients.All.updateGuid(guid);

            Clients.All.updateProgress("Starting Backup Progress");

            // Add an entry to the Cache
            // Application is deprecated by Cache. 
            // If you need something with application scope, then you should either create it as a static member of a class or use the Cache

            // PLAN B - Create a Cache Item for the GUID and keep adding to the item for the backup.
            HttpRuntime.Cache.Insert(guid.ToString(), DateTime.Now.ToString() + ": Backup Started.", null, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration,System.Web.Caching.CacheItemPriority.Normal,null);

            // Update the Cache 100 000 times 
            // THIS PERSISTS WHEN THE BROWSER IS CLOSED!
            //for (int i = 1; i < 100000; i++)
            //{
            //    HttpRuntime.Cache[guid.ToString()] = " Copying " + i.ToString() + " Files....." + HttpRuntime.Cache[guid.ToString()].ToString() + "</br>";
            //}

            // Call The Backup as a Callback Function on the server
            TheBackupStuff(guid);

            // We continue Here.....
            // int a = 1;


        }

        public void showFeedback(String TheGuid)
        {
            // Clears the Progress feedback on the Client Screen.
            Clients.All.clearProgress();

            // Retrieves the value from the Cache
            if (HttpRuntime.Cache[TheGuid.ToString()] == null)
            {
                Clients.All.updateProgress("No Log Detail found for the identifier specified:");
            }
            else
            {
                Clients.All.updateProgress(HttpRuntime.Cache[TheGuid.ToString()].ToString());
            }

        }

        // This does the backup, but ASYNCRONIOUSLY //
        //
        private async Task TheBackupStuff(string Guid)
        {

            // Log in the cache 
            await DOTheRealBackupHere(Guid);
            // The rest from here is a callback function to the above TASK!

        }

        public async Task  DOTheRealBackupHere(string guid)  // this must return an awaitable....

        {

            BackupUtilites.BackupSync.Main(TheSourcePath, TheTargetPath, guid);

        }


    }
    }
