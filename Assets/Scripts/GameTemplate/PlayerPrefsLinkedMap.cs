using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace {
    public class PlayerPrefsLinkedMap {
        private string headKey {
            get => PlayerPrefs.GetString($"PPLM.{id}.headKey", "");
            set => PlayerPrefs.SetString($"PPLM.{id}.headKey", value);
        }
        private string tailKey {
            get => PlayerPrefs.GetString($"PPLM.{id}.tailKey", "");
            set => PlayerPrefs.SetString($"PPLM.{id}.tailKey", value);
        }

        private readonly string id;
        
        public PlayerPrefsLinkedMap(string id) {
            this.id = id;
        }

        public List<string> keys() {
            List<string> keys = new List<string>();
            PlayerPrefsLinkedListNode node = findNode(headKey);
            while (node != null) {
                keys.Add(node.key);
                node = findNode(node.nextKey);
            }
            return keys;
        }
        
        private PlayerPrefsLinkedListNode findNode(string key) {
            if (key == "") return null;
            if (PlayerPrefs.HasKey($"PPLM.{id}.{key}") == false) return null;
            string nodeText = PlayerPrefs.GetString($"PPLM.{id}.{key}");
            PlayerPrefsLinkedListNode node = new PlayerPrefsLinkedListNode();
            node.fromString(nodeText);
            return node;
        }
        
        private void add(string key, string value) {
            PlayerPrefsLinkedListNode newNode = new PlayerPrefsLinkedListNode {
                key = key,
                value = value,
                nextKey = ""
            };
            if (headKey == "") headKey = newNode.key;
            if (tailKey == "") tailKey = newNode.key; 
            else {
                PlayerPrefsLinkedListNode tail = findNode(tailKey);
                tail.nextKey = newNode.key;
                updateData(tail);
                tailKey = newNode.key;
            }
            updateData(newNode);
        }
        
        public void update(string key, string value) {
            PlayerPrefsLinkedListNode node = findNode(key);
            if (node == null) {
                add(key, value);
                return;
            }
            node.value = value;
            updateData(node);
        }
        
        public string getOrDefault(string key, string defaultVar) {
            PlayerPrefsLinkedListNode node = findNode(key);
            if (node == null) return defaultVar;
            return node.value;
        }

        private void updateData(PlayerPrefsLinkedListNode node) {
            PlayerPrefs.SetString($"PPLM.{id}.{node.key}", node.toString());
        }

        public bool hasKey(string playerName) {
            return findNode(playerName) != null;
        }
    }
    
    public class PlayerPrefsLinkedListNode {
        public string key;
        public string value;
        public string nextKey = "";
        
        public string toString() {
            return $"{key}:{value}:{nextKey}";
        }
        
        public void fromString(string str) {
            string[] parts = str.Split(':');
            this.key = parts[0];
            this.value = parts[1];
            if (parts[2] != "") this.nextKey = parts[2];
        }
    }
}