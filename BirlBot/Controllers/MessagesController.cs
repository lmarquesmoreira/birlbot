using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.Dialogs.Internals;
using System.Diagnostics;
using Autofac;

namespace BirlBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

            if (activity != null)
            {
                // one of these will have an interface and process it
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                        await Conversation.SendAsync(activity, () => BirlDialog.birlDialog);
                        break;

                    case ActivityTypes.ConversationUpdate:
                        IConversationUpdateActivity update = activity;
                        using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                        {
                            var client = scope.Resolve<IConnectorClient>();
                            if (update.MembersAdded.Any())
                            {
                                var reply = activity.CreateReply();
                                foreach (var newMember in update.MembersAdded)
                                {
                                    if (newMember.Id != activity.Recipient.Id)
                                    {
                                        reply.Text = $"Ola {newMember.Name}!";
                                    }
                                    else
                                    {
                                        reply.Text = $"Ola {activity.From.Name}";
                                    }
                                    await client.Conversations.ReplyToActivityAsync(reply);
                                }
                            }
                        }
                        break;
                    case ActivityTypes.ContactRelationUpdate:
                    case ActivityTypes.Typing:
                    case ActivityTypes.DeleteUserData:
                    case ActivityTypes.Ping:
                    default:
                        Trace.TraceError($"Unknown activity type ignored: {activity.GetActivityType()}");
                        break;
                }
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {

                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }

    class BirlDialog
    {
        public static readonly IDialog<string> birlDialog = Chain.PostToChain()
    .Switch(
    new Case<IMessageActivity, IDialog<string>>(msg =>
    {
        return msg.Text.ToLower().Equals("HORA DO SHOW".ToLower());
    }, (context, txt) =>
    {
        return Chain.Return("O QUE CE QUER MONSTRAO?");
    }),
    new Case<IMessageActivity, IDialog<string>>(msg =>
    {
        return match(msg.Text, "VAMO MONSTRO");
    }, (context, msg) =>
    {

        if (match(msg.Text, "oferta"))
            return verOfertas(context, msg);
        else if (match(msg.Text, "quanto perguntei"))
            return quantoContei(context, msg);
        else if (match(msg.Text, "comprar"))
            return comprar(context, msg);
        else
            return Chain.Return("FALA DIREITO MONSTRO");
    }),
     new DefaultCase<IMessageActivity, IDialog<string>>((ctx, msg) =>
     {
         return Chain.Return("FALA DIREITO MONSTRO, termine a frase com Vamo monstro");
     }))
    .Unwrap().PostToUser();

        static Func<string, string, bool> match = (msg, str) => msg.ToLower().Contains(str.ToLower());

        static Action<IBotContext> addCount = (ctx) =>
        {
            int count;
            ctx.UserData.TryGetValue("count", out count);
            ctx.UserData.SetValue("count", ++count);
        };

        static Func<IBotContext, IMessageActivity, IDialog<string>> verOfertas = (ctx, x) =>
        {
            addCount(ctx);
            // insira alguma logica
            return Chain.Return("AJUDA O MALUCO QUE TA DOENTE - Oferta X");
        };

        static Func<IBotContext, IMessageActivity, IDialog<string>> quantoContei = (ctx, x) =>
        {
            addCount(ctx);
            int count;
            ctx.UserData.TryGetValue("count", out count);
            string reply = string.Format("CE PERGUNTOU {0} vezes MONSTRAO", count);
            // insira alguma logica
            return Chain.Return(reply);
        };

        static Func<IBotContext, IMessageActivity, IDialog<string>> comprar = (ctx, x) =>
        {
            addCount(ctx);
            // insira alguma logica
            return Chain.Return("VAMO COMPRA MONSTRO;");
        };
    }
}