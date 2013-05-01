using UnityEngine;
using System;
using System.Runtime.InteropServices;


namespace com.soomla.unity
{
	/// <summary>
	/// You can use this class to purchase products from the native phone market, buy virtual goods, and do many other store related operations.
	/// </summary>
	public class StoreController
	{
		private const string TAG = "SOOMLA StoreController";
#if UNITY_IOS
		[DllImport ("__Internal")]
		private static extern void storeController_Init(string customSecret);
		[DllImport ("__Internal")]
		private static extern int storeController_BuyMarketItem(string productId);
		[DllImport ("__Internal")]
		private static extern void storeController_StoreOpening();
		[DllImport ("__Internal")]
		private static extern void storeController_StoreClosing();
		[DllImport ("__Internal")]
		private static extern void storeController_RestoreTransactions();
		[DllImport ("__Internal")]
		private static extern void storeController_SetSoomSec(string soomSec);
#endif
		
#if UNITY_ANDROID
		private static AndroidJavaObject jniStoreController = null;
//		private static AndroidJavaObject jniUnityEventHandler = null;
#endif
		
		public static void Initialize(IStoreAssets storeAssets) {
			if (string.IsNullOrEmpty(Soomla.GetInstance().publicKey) || string.IsNullOrEmpty(Soomla.GetInstance().customSecret) || string.IsNullOrEmpty(Soomla.GetInstance().soomSec)) {
				StoreUtils.LogError(TAG, "SOOMLA/UNITY MISSING publickKey or customSecret or soomSec !!! Stopping here !!");
				throw new ExitGUIException();
			}
			//init SOOM_SEC
#if UNITY_ANDROID
			AndroidJNI.PushLocalFrame(100);
			using(AndroidJavaClass jniStoreAssets = new AndroidJavaClass("com.soomla.unity.StoreAssets")) {
				jniStoreAssets.CallStatic("setSoomSec", Soomla.GetInstance().soomSec);
			}
			AndroidJNI.PopLocalFrame(IntPtr.Zero);
#elif UNITY_IOS
			storeController_SetSoomSec(Soomla.GetInstance().soomSec);
#endif
			
			StoreInfo.Initialize(storeAssets);
#if UNITY_ANDROID
			AndroidJNI.PushLocalFrame(100);
			using(AndroidJavaObject jniStoreAssetsInstance = new AndroidJavaObject("com.soomla.unity.StoreAssets")) {
				using(AndroidJavaClass jniStoreControllerClass = new AndroidJavaClass("com.soomla.store.StoreController")) {
					jniStoreController = jniStoreControllerClass.CallStatic<AndroidJavaObject>("getInstance");
					jniStoreController.Call("initialize", jniStoreAssetsInstance, Soomla.GetInstance().publicKey, Soomla.GetInstance().customSecret);
				}
			}
			//init EventHandler
			using(AndroidJavaClass jniEventHandler = new AndroidJavaClass("com.soomla.unity.EventHandler")) {
				jniEventHandler.CallStatic("initialize");
			}
			AndroidJNI.PopLocalFrame(IntPtr.Zero);
#elif UNITY_IOS
			storeController_Init(Soomla.GetInstance().customSecret);
#endif
		}
		
		
		public static void BuyMarketItem(string productId) {
#if UNITY_ANDROID
			AndroidJNI.PushLocalFrame(100);
			using(AndroidJavaObject jniPurchasableItem = AndroidJNIHandler.CallStatic<AndroidJavaObject>(
				new AndroidJavaClass("com.soomla.unity.StoreInfo"),"getPurchasableItem", productId)) {
				AndroidJNIHandler.CallVoid(jniStoreController, "buyWithGooglePlay", 
					jniPurchasableItem.Call<AndroidJavaObject>("getPurchaseType").Call<AndroidJavaObject>("getGoogleMarketItem"), 
					"");
			}
			AndroidJNI.PopLocalFrame(IntPtr.Zero);
#elif UNITY_IOS
			storeController_BuyMarketItem(productId);
#endif
		}
		
		public static void StoreOpening() {
			if(!Application.isEditor){
#if UNITY_ANDROID
				AndroidJNI.PushLocalFrame(100);
				using(AndroidJavaClass jniUnityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer")){
					using(AndroidJavaObject jniCurrentActivity = jniUnityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity")) {
						jniStoreController.Call("storeOpening", jniCurrentActivity);
					}
				}
				AndroidJNI.PopLocalFrame(IntPtr.Zero);
#elif UNITY_IOS
				storeController_StoreOpening();
#endif
			}
		}
		
		public static void StoreClosing() {
			if(!Application.isEditor){
#if UNITY_ANDROID
				AndroidJNI.PushLocalFrame(100);
				jniStoreController.Call("storeClosing");
				AndroidJNI.PopLocalFrame(IntPtr.Zero);
#elif UNITY_IOS
				storeController_StoreClosing();
#endif
			}
		}
		
		public static void RestoreTransactions() {
			if(!Application.isEditor){
#if UNITY_ANDROID
				AndroidJNI.PushLocalFrame(100);
				jniStoreController.Call("restoreTransactions");
				AndroidJNI.PopLocalFrame(IntPtr.Zero);
#elif UNITY_IOS
				storeController_RestoreTransactions();
#endif
			}
		}
		
	}
}

