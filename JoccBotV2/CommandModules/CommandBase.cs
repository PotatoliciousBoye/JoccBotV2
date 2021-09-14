using Discord.Commands;
using System.Threading.Tasks;

namespace JoccBotV2.CommandModules
{
    /// <summary>
    /// Defines the <see cref="CommandBase" />.
    /// </summary>
    public class CommandBase : ModuleBase<SocketCommandContext>
    {
        #region Methods

        /// <summary>
        /// The Ping.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong");
        }

        #endregion
    }
}
