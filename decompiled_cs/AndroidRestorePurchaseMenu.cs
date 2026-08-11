using System.Collections.Generic;
using SFS.Sharing;
using SFS.UI;
using UnityEngine.UI;

public class AndroidRestorePurchaseMenu : BasicMenu
{
	public Text signInOutButtonText;

	public Text currentEmailText;

	public Text recoveredPurchasesText;

	public SFS.UI.Button addIdButton;

	private RequestUtil requestUtil = new RequestUtil();

	private Dictionary<string, string> registeredPurchases = new Dictionary<string, string>();
}
