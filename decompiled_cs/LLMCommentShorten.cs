public class LLMCommentShorten : LLMComment
{
	public LLMCommentShorten(string comment)
		: base("If this translates with too many characters, shorten it to " + comment)
	{
	}
}
