namespace ModLoader;

public interface IMod
{
	string DisplayName { get; }

	string Description { get; }

	string Author { get; }

	void Load();
}
