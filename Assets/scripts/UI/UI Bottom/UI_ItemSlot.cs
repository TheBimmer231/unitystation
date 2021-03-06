﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using Events;
using PlayGroup;
using UnityEngine.Events;
using Items;

namespace UI {

    public class UI_ItemSlot: MonoBehaviour {

        public string eventName;
        public bool allowAllItems;
        public List<ItemType> allowedItemTypes;
        public ItemSize maxItemSize;

        private Image image;

        public GameObject Item { get; private set; }
        public bool IsFull {
            get {
                return Item != null;
            }
        }

        void Awake() {
            image = GetComponent<Image>();
            image.enabled = false;
            if (eventName.Length > 0)
            {
//                Debug.LogErrorFormat("Triggered SetItem for {0}",eventName);
                EventManager.UI.AddListener(eventName, x => TrySetItem(x));
            }
        }

		void OnEnable()
		{
			SceneManager.sceneLoaded += OnLevelFinishedLoading;
		}

		void OnDisable()
		{
			SceneManager.sceneLoaded -= OnLevelFinishedLoading;
		}

		//Reset Item slot sprite on game restart
		void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode){
			image.sprite = null;
			image.enabled = false;
		}

        public void SetItem(GameObject item) {
            if (PlayerManager.LocalPlayerScript != null)
                if (!PlayerManager.LocalPlayerScript.playerMove.allowInput || PlayerManager.LocalPlayerScript.playerMove.isGhost)
                    return;

            image.sprite = item.GetComponentInChildren<SpriteRenderer>().sprite;
            image.enabled = true;
            Item = item;
            item.transform.position = transform.position;
			if (PlayerManager.LocalPlayer != null && item != null) {
				PlayerManager.LocalPlayerScript.playerNetworkActions.CmdSetUISlot(eventName, item);
			}
//            if(eventName.Length > 0)
//                EventManager.UI.TriggerEvent(eventName, item);
        }

        public bool TrySetItem(GameObject item) {
            if(!IsFull && item != null && CheckItemFit(item)) {
//                Debug.LogErrorFormat("TrySetItem TRUE for {0}", item.GetComponent<ItemAttributes>().hierarchy);
                SetItem(item);

                return true;
            }
//            Debug.LogErrorFormat("TrySetItem FALSE for {0}", item.GetComponent<ItemAttributes>().hierarchy);
            return false;
        }

        /// <summary>
        /// removes item from slot
        /// </summary>
        /// <returns></returns>
        public GameObject Clear() {
            if (PlayerManager.LocalPlayerScript != null)
                if (!PlayerManager.LocalPlayerScript.playerMove.allowInput || PlayerManager.LocalPlayerScript.playerMove.isGhost)
                    return null;

            var item = Item;
            Item = null;

//            if(eventName.Length > 0 && item != null)
//                EventManager.UI.TriggerEvent(eventName, null);
            PlayerManager.LocalPlayerScript.playerNetworkActions.CmdClearUISlot(eventName);
            image.sprite = null;
            image.enabled = false;

            return item;
        }

		/// <summary>
		/// returnes the current item from the slot
		/// </summary>
		/// <returns></returns>
		public GameObject GameObject() {
			return Item;
		}

        /// <summary>
        /// for use with specific item placement (tables/cupboards etc);
        /// </summary>
        /// <returns></returns>
        public GameObject PlaceItemInScene() {
            if (PlayerManager.LocalPlayerScript != null)
                if (!PlayerManager.LocalPlayerScript.playerMove.allowInput || PlayerManager.LocalPlayerScript.playerMove.isGhost)
                    return null;

            var item = Item;
            Item = null;
            image.sprite = null;
            image.enabled = false;

            return item;
        }

        public void Reset() {
			image.sprite = null;
			image.enabled = false;
			Item = null;
        }

        private bool CheckItemFit(GameObject item) {
            var attributes = item.GetComponent<ItemAttributes>();
            if(!allowAllItems) {
                if(!allowedItemTypes.Contains(attributes.type)) {
                    return false;
                } //fixme: following code prevents player from holding/wearing stuff that is wearable in /tg/ 
            }/*else if(maxItemSize != ItemSize.Large && (maxItemSize != ItemSize.Medium || attributes.size == ItemSize.Large) && maxItemSize != attributes.size) {
                Debug.Log("Item is too big!");
                return false;
            }*/

            return true;
        }
    }
}