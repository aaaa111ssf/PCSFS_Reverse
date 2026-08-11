using System;

public class LLMComment : Attribute
{
	public string comment;

	public LLMComment(string comment)
	{
		this.comment = comment;
	}
}
