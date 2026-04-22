namespace Sery.Application.Chat;

public interface IChatPromptComposer
{
    string ComposePrompt(ChatPromptContext context);
}
