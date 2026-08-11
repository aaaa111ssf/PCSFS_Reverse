using System;
using SFS.Input;
using UnityEngine;
using UnityEngine.UI;

namespace SFS.UI;

public class ShopManager : MonoBehaviour
{
	[Serializable]
	public class BuyButton
	{
		public Button button;
	}

	public RectTransform saleText_Home;

	public RectTransform saleText_BuyButton;

	[Space]
	public Screen_Menu shopMenu;

	public ProductThumbnail partsThumbnail;

	public ProductThumbnail redstoneAtlasPackThumbnail;

	public ProductThumbnail skinsThumbnail;

	public ProductThumbnail gasGiantsThumbnail;

	public ProductThumbnail iceGiantsThumbnail;

	public ProductThumbnail cheatsThumbnail;

	public ProductThumbnail infiniteAreaThumbnail;

	public ProductThumbnail newFullBundleThumbnail;

	public ScrollElement packsScroller;

	public ScrollElement bundlesScroller;

	public Button restoreButton;

	public Button restoreButton2;

	public AndroidRestorePurchaseMenu restoreMenu;

	public BuyButton buyButton_Parts;

	public BuyButton buyButton_RedstoneAtlasPack;

	public BuyButton buyButton_Skins;

	public BuyButton buyButton_GasGiants;

	public BuyButton buyButton_IceGiants;

	public BuyButton buyButton_Cheats;

	public BuyButton buyButton_InfiniteArea;

	public BuyButton buyButton_NewFullBundle;

	public static bool showThanksMessage;

	public Text cheatsContent_1;

	public Text cheatsContent_4;

	public Text partsTextNF;

	public Text redstoneAtlasPackNF;

	public Text skinsTextNF;

	public Text gasGiantsTextNF;

	public Text iceGiantsTextNF;

	public Text cheatsTextNF;

	public Text infiniteAreaBundleNF;
}
