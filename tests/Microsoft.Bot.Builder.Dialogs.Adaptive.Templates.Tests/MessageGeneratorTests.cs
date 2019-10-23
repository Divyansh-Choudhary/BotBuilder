﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Tests;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class MessageGeneratorTests
    {
        private static ResourceExplorer resourceExplorer;

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            DeclarativeTypeLoader.AddComponent(new AdaptiveComponentRegistration());
            DeclarativeTypeLoader.AddComponent(new LanguageGenerationComponentRegistration());

            resourceExplorer = ResourceExplorer.LoadProject(GetProjectFolder());
        }

        [TestMethod]
        public async Task TestInlineActivityGenerator()
        {
            var context = GetTurnContext(new MockLanguageGenerator());
            var mg = new ActivityGenerator();
            var activity = await mg.Generate(context, "text", data: null) as Activity;
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("text", activity.Text);
            Assert.AreEqual("text", activity.Speak);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task TestNotSupportStructuredType()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var mg = new ActivityGenerator();
            var result = await mg.Generate(context, "[notSupport]", null) as Activity;
        }

        [TestMethod]
        public async Task ActivityGeneratorTest()
        {
            var context = await GetTurnContext("NormalStructuredLG.lg");
            var mg = new ActivityGenerator();
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";

            var activity = await mg.Generate(context, "[HerocardWithCardAction]", data: data) as Activity;
            AssertCardActionActivity(activity);

            data.adaptiveCardTitle = "test";
            activity = await mg.Generate(context, "[adaptivecardActivity]", data: data) as Activity;
            AssertAdaptiveCardActivity(activity);

            activity = await mg.Generate(context, "[eventActivity]", data: data) as Activity;
            AssertEventActivity(activity);

            activity = await mg.Generate(context, "[activityWithHeroCardAttachment]", data: data) as Activity;
            AssertActivityWithHeroCardAttachment(activity);

            activity = await mg.Generate(context, "[activityWithMultiAttachments]", data: data) as Activity;
            AssertActivityWithMultiAttachments(activity);

            activity = await mg.Generate(context, "[activityWithSuggestionActions]", data: data) as Activity;
            AssertActivityWithSuggestionActions(activity);

            activity = await mg.Generate(context, "[messageActivityAll]", data: data) as Activity;
            AssertMessageActivityAll(activity);

            activity = await mg.Generate(context, "[activityWithMultiStructuredSuggestionActions]", data: data) as Activity;
            AssertActivityWithMultiStructuredSuggestionActions(activity);

            activity = await mg.Generate(context, "[activityWithMultiStringSuggestionActions]", data: data) as Activity;
            AssertActivityWithMultiStringSuggestionActions(activity);

            data.type = "herocard";
            activity = await mg.Generate(context, "[HeroCardTemplate]", data: data) as Activity;
            AssertHeroCardActivity(activity);

            data.type = "thumbnailcard";
            activity = await mg.Generate(context, "[ThumbnailCardTemplate]", data: data) as Activity;
            AssertThumbnailCardActivity(activity);

            data.type = "audiocard";
            activity = await mg.Generate(context, "[AudioCardTemplate]", data: data) as Activity;
            AssertAudioCardActivity(activity);

            data.type = "videocard";
            activity = await mg.Generate(context, "[VideoCardTemplate]", data: data) as Activity;
            AssertVideoCardActivity(activity);

            data.signinlabel = "Sign in";
            data.url = "https://login.microsoftonline.com/";
            activity = await mg.Generate(context, "[SigninCardTemplate]", data: data) as Activity;
            AssertSigninCardActivity(activity);

            data.connectionName = "MyConnection";
            activity = await mg.Generate(context, "[OAuthCardTemplate]", data: data) as Activity;
            AssertOAuthCardActivity(activity);
        }

        [TestMethod]
        public void TestGenerateFromLG()
        {
            var r = GetLGTFilePath("NormalStructuredLG.lg");
            dynamic data = new JObject();
            data.title = "titleContent";
            data.text = "textContent";

            var engine = new TemplateEngine().AddFile(GetLGTFilePath("NormalStructuredLG.lg"));

            var activity = ActivityGenerator.GenerateFromLG(engine.EvaluateTemplate("HerocardWithCardAction", data));
            AssertCardActionActivity(activity);

            data.adaptiveCardTitle = "test";
            activity = ActivityGenerator.GenerateFromLG(engine.EvaluateTemplate("adaptivecardActivity", data));
            AssertAdaptiveCardActivity(activity);

            activity = ActivityGenerator.GenerateFromLG(engine.EvaluateTemplate("eventActivity", data));
            AssertEventActivity(activity);

            activity = ActivityGenerator.GenerateFromLG(engine.EvaluateTemplate("activityWithHeroCardAttachment", data));
            AssertActivityWithHeroCardAttachment(activity);

            activity = ActivityGenerator.GenerateFromLG(engine.EvaluateTemplate("activityWithMultiAttachments", data));
            AssertActivityWithMultiAttachments(activity);

            activity = ActivityGenerator.GenerateFromLG(engine.EvaluateTemplate("activityWithSuggestionActions", data));
            AssertActivityWithSuggestionActions(activity);

            activity = ActivityGenerator.GenerateFromLG(engine.EvaluateTemplate("messageActivityAll", data));
            AssertMessageActivityAll(activity);

            activity = ActivityGenerator.GenerateFromLG(engine.EvaluateTemplate("activityWithMultiStructuredSuggestionActions", data));
            AssertActivityWithMultiStructuredSuggestionActions(activity);

            activity = ActivityGenerator.GenerateFromLG(engine.EvaluateTemplate("activityWithMultiStringSuggestionActions", data));
            AssertActivityWithMultiStringSuggestionActions(activity);

            data.type = "herocard";
            activity = ActivityGenerator.GenerateFromLG(engine.EvaluateTemplate("HeroCardTemplate", data));
            AssertHeroCardActivity(activity);

            data.type = "thumbnailcard";
            activity = ActivityGenerator.GenerateFromLG(engine.EvaluateTemplate("ThumbnailCardTemplate", data));
            AssertThumbnailCardActivity(activity);

            data.type = "audiocard";
            activity = ActivityGenerator.GenerateFromLG(engine.EvaluateTemplate("AudioCardTemplate", data));
            AssertAudioCardActivity(activity);

            data.type = "videocard";
            activity = ActivityGenerator.GenerateFromLG(engine.EvaluateTemplate("VideoCardTemplate", data));
            AssertVideoCardActivity(activity);

            data.signinlabel = "Sign in";
            data.url = "https://login.microsoftonline.com/";
            activity = ActivityGenerator.GenerateFromLG(engine.EvaluateTemplate("SigninCardTemplate", data));
            AssertSigninCardActivity(activity);

            data.connectionName = "MyConnection";
            activity = ActivityGenerator.GenerateFromLG(engine.EvaluateTemplate("OAuthCardTemplate", data));
            AssertOAuthCardActivity(activity);
        }

        private static string GetProjectFolder()
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
        }

        private void AssertMessageActivityAll(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("textContent", activity.Text);
            Assert.AreEqual("textContent", activity.Speak);
            Assert.AreEqual("accepting", activity.InputHint);
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(AttachmentLayoutTypes.List, activity.AttachmentLayout);
            Assert.AreEqual(HeroCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<HeroCard>();
            Assert.IsNotNull(card, "should have herocard");
            Assert.AreEqual("titleContent", card.Title, "card title should be set");
            Assert.AreEqual("textContent", card.Text, "card text should be set");
            Assert.AreEqual(1, card.Buttons.Count, "card buttons should be set");
            Assert.AreEqual($"imBack", card.Buttons[0].Type, "card buttons should be set");
            Assert.AreEqual($"titleContent", card.Buttons[0].Title, "card buttons should be set");
            Assert.AreEqual($"textContent", card.Buttons[0].Value, "card buttons should be set");
            Assert.AreEqual(activity.SuggestedActions.Actions.Count, 2);
            Assert.AreEqual(activity.SuggestedActions.Actions[0].DisplayText, "firstItem");
            Assert.AreEqual(activity.SuggestedActions.Actions[0].Title, "firstItem");
            Assert.AreEqual(activity.SuggestedActions.Actions[0].Text, "firstItem");
            Assert.AreEqual(activity.SuggestedActions.Actions[1].Title, "titleContent");
            Assert.AreEqual(activity.SuggestedActions.Actions[1].Value, "textContent");
        }

        private void AssertActivityWithSuggestionActions(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("textContent", activity.Text);
            Assert.AreEqual(activity.SuggestedActions.Actions.Count, 2);
            Assert.AreEqual(activity.SuggestedActions.Actions[0].DisplayText, "firstItem");
            Assert.AreEqual(activity.SuggestedActions.Actions[0].Title, "firstItem");
            Assert.AreEqual(activity.SuggestedActions.Actions[0].Text, "firstItem");
            Assert.AreEqual(activity.SuggestedActions.Actions[1].Title, "titleContent");
            Assert.AreEqual(activity.SuggestedActions.Actions[1].Value, "textContent");
        }

        private void AssertActivityWithMultiAttachments(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(2, activity.Attachments.Count);
            Assert.AreEqual(ThumbnailCard.ContentType, activity.Attachments[1].ContentType);
            var card = ((JObject)activity.Attachments[1].Content).ToObject<ThumbnailCard>();
            Assert.IsNotNull(card, "should have herocard");
            Assert.AreEqual("Cheese gromit!", card.Title, "card title should be set");
            Assert.AreEqual("type", card.Subtitle, "card subtitle should be data bound ");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[0].Url, "image should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[1].Url, "image should be set");
            Assert.AreEqual(3, card.Buttons.Count, "card buttons should be set");
            for (int i = 0; i <= 2; i++)
            {
                Assert.AreEqual($"Option {i + 1}", card.Buttons[i].Title, "card buttons should be set");
            }
        }

        private void AssertActivityWithHeroCardAttachment(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(HeroCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<HeroCard>();
            Assert.IsNotNull(card, "should have herocard");
            Assert.AreEqual("titleContent", card.Title, "card title should be set");
            Assert.AreEqual("textContent", card.Text, "card text should be set");
            Assert.AreEqual(1, card.Buttons.Count, "card buttons should be set");
            Assert.AreEqual($"imBack", card.Buttons[0].Type, "card buttons should be set");
            Assert.AreEqual($"titleContent", card.Buttons[0].Title, "card buttons should be set");
            Assert.AreEqual($"textContent", card.Buttons[0].Value, "card buttons should be set");
        }

        private void AssertEventActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Event, activity.Type);
            Assert.AreEqual("textContent", activity.Name, "card name should be set");
            Assert.AreEqual("textContent", activity.Value, "card value should be set");
        }

        private void AssertAdaptiveCardActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual("application/vnd.microsoft.card.adaptive", activity.Attachments[0].ContentType);
            Assert.AreEqual("test", (string)((dynamic)activity.Attachments[0].Content).body[0].text);
        }

        private void AssertCardActionActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(HeroCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<HeroCard>();
            Assert.IsNotNull(card, "should have herocard");
            Assert.AreEqual("titleContent", card.Title, "card title should be set");
            Assert.AreEqual("textContent", card.Text, "card text should be set");
            Assert.AreEqual(1, card.Buttons.Count, "card buttons should be set");
            Assert.AreEqual($"imBack", card.Buttons[0].Type, "card buttons should be set");
            Assert.AreEqual($"titleContent", card.Buttons[0].Title, "card buttons should be set");
            Assert.AreEqual($"textContent", card.Buttons[0].Value, "card buttons should be set");
        }

        private void AssertThumbnailCardActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(ThumbnailCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<ThumbnailCard>();
            Assert.IsNotNull(card, "should have herocard");
            Assert.AreEqual("Cheese gromit!", card.Title, "card title should be set");
            Assert.AreEqual("thumbnailcard", card.Subtitle, "card subtitle should be data bound ");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[0].Url, "image should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[1].Url, "image should be set");
            Assert.AreEqual(3, card.Buttons.Count, "card buttons should be set");
            for (int i = 0; i <= 2; i++)
            {
                Assert.AreEqual($"Option {i + 1}", card.Buttons[i].Title, "card buttons should be set");
            }
        }

        private void AssertHeroCardActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(HeroCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<HeroCard>();
            Assert.IsNotNull(card, "should have herocard");
            Assert.AreEqual("Cheese gromit!", card.Title, "card title should be set");
            Assert.AreEqual("herocard", card.Subtitle, "card subtitle should be data bound ");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[0].Url, "image should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Images[1].Url, "image should be set");
            Assert.AreEqual(3, card.Buttons.Count, "card buttons should be set");
            for (int i = 0; i <= 2; i++)
            {
                Assert.AreEqual($"Option {i + 1}", card.Buttons[i].Title, "card buttons should be set");
            }
        }

        private void AssertAudioCardActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(AudioCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<AudioCard>();
            Assert.IsNotNull(card, "should have audiocard");
            Assert.AreEqual("Cheese gromit!", card.Title, "card title should be set");
            Assert.AreEqual("audiocard", card.Subtitle, "card subtitle should be data bound ");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Image.Url, "image should be set");
            Assert.AreEqual("https://contoso.com/media/AllegrofromDuetinCMajor.mp3", card.Media[0].Url);
            Assert.AreEqual(false, card.Shareable);
            Assert.AreEqual(true, card.Autoloop);
            Assert.AreEqual(true, card.Autostart);
            Assert.AreEqual("16:9", card.Aspect);
            Assert.AreEqual(3, card.Buttons.Count, "card buttons should be set");
            for (int i = 0; i <= 2; i++)
            {
                Assert.AreEqual($"Option {i + 1}", card.Buttons[i].Title, "card buttons should be set");
            }
        }

        private void AssertVideoCardActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(VideoCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<VideoCard>();
            Assert.IsNotNull(card, "should have videocard");
            Assert.AreEqual("Cheese gromit!", card.Title, "card title should be set");
            Assert.AreEqual("videocard", card.Subtitle, "card subtitle should be data bound ");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual("https://memegenerator.net/img/instances/500x/73055378/cheese-gromit.jpg", card.Image.Url, "image should be set");
            Assert.AreEqual("https://youtu.be/530FEFogfBQ", card.Media[0].Url);
            Assert.AreEqual(false, card.Shareable);
            Assert.AreEqual(true, card.Autoloop);
            Assert.AreEqual(true, card.Autostart);
            Assert.AreEqual("16:9", card.Aspect);
            Assert.AreEqual(3, card.Buttons.Count, "card buttons should be set");
            for (int i = 0; i <= 2; i++)
            {
                Assert.AreEqual($"Option {i + 1}", card.Buttons[i].Title, "card buttons should be set");
            }
        }

        private void AssertSigninCardActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(SigninCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<SigninCard>();
            Assert.IsNotNull(card, "should have signincard");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual(1, card.Buttons.Count, "card buttons should be set");
            Assert.AreEqual($"Sign in", card.Buttons[0].Title);
            Assert.AreEqual(ActionTypes.Signin, card.Buttons[0].Type);
            Assert.AreEqual($"https://login.microsoftonline.com/", card.Buttons[0].Value);
        }

        private void AssertOAuthCardActivity(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.IsTrue(string.IsNullOrEmpty(activity.Text));
            Assert.IsTrue(string.IsNullOrEmpty(activity.Speak));
            Assert.AreEqual(1, activity.Attachments.Count);
            Assert.AreEqual(OAuthCard.ContentType, activity.Attachments[0].ContentType);
            var card = ((JObject)activity.Attachments[0].Content).ToObject<OAuthCard>();
            Assert.IsNotNull(card, "should have signincard");
            Assert.AreEqual("This is some text describing the card, it's cool because it's cool", card.Text, "card text should be set");
            Assert.AreEqual("MyConnection", card.ConnectionName);
            Assert.AreEqual(1, card.Buttons.Count, "card buttons should be set");
            Assert.AreEqual($"Sign in", card.Buttons[0].Title);
            Assert.AreEqual(ActionTypes.Signin, card.Buttons[0].Type);
            Assert.AreEqual($"https://login.microsoftonline.com/", card.Buttons[0].Value);
        }

        private void AssertActivityWithMultiStructuredSuggestionActions(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("textContent", activity.Text);
            Assert.AreEqual(activity.SuggestedActions.Actions.Count, 3);
            Assert.AreEqual(activity.SuggestedActions.Actions[0].Value, "first suggestion");
            Assert.AreEqual(activity.SuggestedActions.Actions[0].Title, "first suggestion");
            Assert.AreEqual(activity.SuggestedActions.Actions[0].Text, "first suggestion");
            Assert.AreEqual(activity.SuggestedActions.Actions[1].Value, "second suggestion");
            Assert.AreEqual(activity.SuggestedActions.Actions[1].Title, "second suggestion");
            Assert.AreEqual(activity.SuggestedActions.Actions[1].Text, "second suggestion");
            Assert.AreEqual(activity.SuggestedActions.Actions[2].Value, "third suggestion");
            Assert.AreEqual(activity.SuggestedActions.Actions[2].Title, "third suggestion");
            Assert.AreEqual(activity.SuggestedActions.Actions[2].Text, "third suggestion");
        }

        private void AssertActivityWithMultiStringSuggestionActions(Activity activity)
        {
            Assert.AreEqual(ActivityTypes.Message, activity.Type);
            Assert.AreEqual("textContent", activity.Text);
            Assert.AreEqual(activity.SuggestedActions.Actions.Count, 3);
            Assert.AreEqual(activity.SuggestedActions.Actions[0].DisplayText, "first suggestion");
            Assert.AreEqual(activity.SuggestedActions.Actions[0].Title, "first suggestion");
            Assert.AreEqual(activity.SuggestedActions.Actions[0].Text, "first suggestion");
            Assert.AreEqual(activity.SuggestedActions.Actions[1].DisplayText, "second suggestion");
            Assert.AreEqual(activity.SuggestedActions.Actions[1].Title, "second suggestion");
            Assert.AreEqual(activity.SuggestedActions.Actions[1].Text, "second suggestion");
            Assert.AreEqual(activity.SuggestedActions.Actions[2].DisplayText, "third suggestion");
            Assert.AreEqual(activity.SuggestedActions.Actions[2].Title, "third suggestion");
            Assert.AreEqual(activity.SuggestedActions.Actions[2].Text, "third suggestion");
        }

        private ITurnContext GetTurnContext(ILanguageGenerator lg)
        {
            var context = new TurnContext(new TestAdapter(), new Activity());
            context.TurnState.Add<ILanguageGenerator>(lg);
            return context;
        }

        private async Task<ITurnContext> GetTurnContext(string lgFile)
        {
            var context = new TurnContext(new TestAdapter(), new Activity());
            var lgText = await resourceExplorer.GetResource(lgFile).ReadTextAsync();
            context.TurnState.Add<ILanguageGenerator>(new TemplateEngineLanguageGenerator(lgText, "test", LanguageGeneratorManager.ResourceResolver(resourceExplorer)));
            return context;
        }

        private string GetLGTFilePath(string fileName)
        {
            return Path.Combine(AppContext.BaseDirectory, "lg", fileName);
        }
    }
}