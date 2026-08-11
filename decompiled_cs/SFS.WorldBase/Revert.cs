using SFS.UI;
using SFS.World;

namespace SFS.WorldBase;

public static class Revert
{
	public static void AddToStack(WorldSave persistent, bool isCareer)
	{
		SavePointer(LoadPointer() + 1);
		WorldSave.Save(GetPath(0), saveRocketsAndBranches: true, persistent, isCareer);
		GetPath(15).Delete();
	}

	public static void DeleteAll()
	{
		Base.worldBase.paths.path.GetFolder("Backup").Delete();
	}

	public static bool HasRevert_30_Sec()
	{
		return HasRevert(2);
	}

	public static bool HasRevert_3_Min()
	{
		return HasRevert(12);
	}

	public static bool TryGetRevert_30_Sec(out WorldSave save)
	{
		return TryGetRevert(2, out save);
	}

	public static bool TryGetRevert_3_Min(out WorldSave save)
	{
		return TryGetRevert(12, out save);
	}

	private static bool HasRevert(int cyclesBack)
	{
		if (GetPath(cyclesBack).Exists())
		{
			return GetPath(cyclesBack).GetFile("Rockets.txt").Exists();
		}
		return false;
	}

	private static bool TryGetRevert(int cyclesBack, out WorldSave save)
	{
		IFolder path = GetPath(cyclesBack);
		if (!path.Exists())
		{
			save = null;
			return false;
		}
		bool result = WorldSave.TryLoad(path, loadRocketsAndBranches: true, MsgDrawer.main, out save);
		for (int num = cyclesBack - 1; num >= 0; num--)
		{
			GetPath(num).Delete();
		}
		SavePointer(LoadPointer() - cyclesBack);
		return result;
	}

	private static IFolder GetPath(int cyclesBack)
	{
		Base.worldBase.paths.path.GetFolder("Backup").Create();
		return Base.worldBase.paths.path.GetFolder("Backup/Backup_" + (LoadPointer() - cyclesBack));
	}

	private static int LoadPointer()
	{
		IFile pointerPath = GetPointerPath();
		if (!pointerPath.Exists() || !int.TryParse(pointerPath.ReadText(), out var result))
		{
			return 0;
		}
		return result;
	}

	private static void SavePointer(int pointer)
	{
		GetPointerPath().WriteText(pointer.ToString());
	}

	private static IFile GetPointerPath()
	{
		Base.worldBase.paths.path.GetFolder("Backup").Create();
		return Base.worldBase.paths.path.GetFile("Backup/RevertPointer.txt");
	}
}
