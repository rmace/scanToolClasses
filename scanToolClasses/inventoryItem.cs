/*
Class Name:  inventoryItem
Description:  Inventory Item
   An inventory item refers to an item that is traked in the PeachTree Sage software's inventory.
   The key identifying feature of the inventory item is its item id.  Item IDs are assigned from
   the item id in the PeachTree Sage software.  The other key element to this class is a list of
   barcodes.  The list of barcodes is used to match all the barcodes that are associated with
   the inventory item.
Purpose:  This class is used to contain the inventory item with all its associated barcodes

Theory of Operation:

Class Level Members:

itemList - At the class level, we keep up with a complete list of every instance of this class as a linked
list, called the itemList.  The itemList can be loaded from a disk file and saved to a disk file.  Changes
in the list can also be merged into a disk file, so that only the changes in the list are stored in the
file.

We keep up with an item's change status.  This is done for the 

    
Date:  08/19/2015
Programmer:  Russell Mace 
Prototype completed on 9/6/2015
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// These are the namespaces we add so that we can reference everything we need.
using System.Xml;
using System.IO;

namespace scanToolClasses
{
    public class inventoryItem
    {
        // Class level members

        // this static list of inventory items is a linked list of all the inventory items we
        // know about.  We implement it as a class-level linked list so we will always be able
        // to search the list when we need to in order to find an item.
        // static public LinkedList<inventoryItem> itemList = new LinkedList<inventoryItem>();

        // this static keyed dictioary list of inventory items is a list of all the
        // inventory items keyed by itemID
        static public Dictionary<string, inventoryItem> itemDictionary = new Dictionary<string, inventoryItem>();

        /// <summary>
        /// Given an xml node, check if the xml node contains a reference to an item id and return
        /// the item id value.  It will return null if no item id is found in the xml node
        /// </summary>
        /// <param name="itemNode"></param>
        /// <returns></returns>
        private static string getItemIDFromXML (XmlNode itemNode)
        {
            String itemText = null;
            foreach (XmlNode node in itemNode.ChildNodes)
            {
                if (node.Name == "Item_ID")
                {
                    itemText = node.InnerText;
                }
            }
            return itemText;
        }

        private static bool doesItemExist (XmlNode itemNode)
        {
            bool itemExists = false;
            String itemText = null;
            itemText = getItemIDFromXML(itemNode);
            if (itemText != null)
            {
                // let's check to see if the item already exists
                if (itemDictionary.ContainsKey(itemText))
                {
                    itemExists = true;
                }
            }
            return itemExists;
        }

        // Given an xml node named "itemNode" with all its child nodes, this
        // method creates a new inventory item and returns a reference for it
        // to the calling program.  If for some reason, the item cannot be
        // created, most likely because the xml is incorrect, then this
        // method returns a null reference.  The Item node must contain an
        // item_id child node, and it can optionally contain a Description
        // child node, and optionally contain a "Barcodes" child node
        private static inventoryItem createItemFromXML(XmlNode itemNode)
        {
            inventoryItem itm = null;
            String itemText = null;
            String descriptText = null;
            foreach (XmlNode node in itemNode.ChildNodes)
            {
                if (node.Name == "Item_ID")
                {
                    itemText = node.InnerText;
                }
                if (node.Name == "Description")
                {
                    descriptText = node.InnerText;
                }
            }
            if (itemText != null)
            {
                // let's check to see if the item already exists
                if (itemDictionary.ContainsKey(itemText))
                {
                    if(itemDictionary.TryGetValue(itemText, out itm))
                    {
                        itm.description = descriptText;
                    }
                }
                else
                {
                    if (descriptText != null)
                    {
                        itm = new inventoryItem(itemText, descriptText);
                    }
                    else
                    {
                        itm = new inventoryItem(itemText);
                    }
                }
            }
            return itm;
        }

        // Given an xml node <Barcodes> with all its child nodes, this
        // method creates a barcode instance for each <Barcode> and 
        // assigns it to the inventory item.
        private static void assignBarcodesToItemFromXML(XmlNode bcList, inventoryItem itm)
        {
            if (bcList != null)
            {
                foreach (XmlNode node in bcList.ChildNodes)
                {
                    if (node.Name == "Barcode")
                    {
                        string bcText = node.InnerText;
                        itm.assignBarcode(bcText);
                    }
                }
            }
        }

        public static int LoadItems(String xmlFile)
        {
            int itemsLoaded = 0;
            // first, let's open the file
            try
            {
                XmlDocument itemFile = new XmlDocument();
                itemFile.Load(xmlFile);

                // now that the file is open, let's parse the xml in it.
                // the file will be full of item nodes, with each item node having
                // a set of child nodes.  Child nodes are item ids, descriptions,
                // and barcodes
                XmlNodeList itms = itemFile.GetElementsByTagName("Item");
                for (int i = 0; i < itms.Count; i++)
                {
                    // the createItemFromXML takes care of the case where
                    // the item already exists.  It will overwrite the
                    // description with the uploaded data.
                    inventoryItem itm = createItemFromXML(itms[i]);
                    itemsLoaded++;
                    XmlNode bcList = itms[i].SelectSingleNode("Barcodes");
                    assignBarcodesToItemFromXML(bcList, itm);
                }
            }
            catch (Exception e)
            {
                // do nothing for right now
            }
            return itemsLoaded;
        }

        public static int saveItems(String fileName)
        {
            int itemsSaved = 0;
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDec, root);
            XmlNode rootNode = doc.CreateElement("Items");
            doc.AppendChild(rootNode);


            // We're going to move through the list of all items and encode each item as
            // an xml node, and then we'll write all the xml nodes to a file.
            foreach (KeyValuePair<string, inventoryItem> entry in inventoryItem.itemDictionary)
            {
                inventoryItem itm = entry.Value;
                XmlNode itmXml = itm.encodeXML(doc);
                rootNode.AppendChild(itmXml);
                itemsSaved++;
            }

            // now save the file
            doc.Save(fileName);
            return itemsSaved;
        }

        /// <summary>
        /// This method merges data from the program into an existing xml file that contains
        /// item data.  If an item exists in our data, then we will overwrite the item in the
        /// file with the new description, and we will add any new barcodes from our data into
        /// the file, but we won't get rid of any existing barcodes that are in the file. 
        /// </summary>
        /// <param name="fileName"></param>
        public static void mergeItemsIntoExistingFile(String fileName)
        {
            // first, let's open the file
            try
            {
                inventoryItem itm = null;
                XmlDocument itemFile = new XmlDocument();
                // By using this filestream object and wrapping all the rest of the code in it,
                // then we make sure that the file is locked during this entire transaction.
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    using (XmlReader xr = XmlReader.Create(fs))
                    {
                        xr.Read();
                        itemFile.Load(xr);
                        // now that the file is open, let's parse the xml in it.
                        // the file will be full of item nodes, but we only want to load the
                        // items that don't already exist 
                        XmlNodeList itms = itemFile.GetElementsByTagName("Item");
                        for (int i = 0; i < itms.Count; i++)
                        {
                            // let's check if this item already exists.
                            if (!doesItemExist(itms[i]))
                            {
                                itm = createItemFromXML(itms[i]);
                            }
                            else
                            // the item already exists, so let's just look it up.
                            {
                                string itemText = getItemIDFromXML(itms[i]);
                                if (itemText != null) itemDictionary.TryGetValue(itemText, out itm);
                            }
                            // coming out of this if statement, if we have a reference to a valid item,
                            // then let's add the barcodes from the file to the valid item.
                            // the assignBarcodesToItemFromXML method only adds barcodes to the item
                            // if they haven't already been added.
                            XmlNode bcList = itms[i].SelectSingleNode("Barcodes");
                            assignBarcodesToItemFromXML(bcList, itm);
                        }
                        saveItems(fileName);
                    }
                    fs.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception("inventoryItem:mergeItemsIntoExistingFile - Error while saving file: " + e.ToString(), e);
            }
        }

        // members
        private String item_id;
        private String description;
        private Dictionary<string, barcode> barcodeList = new Dictionary<string, barcode>();

        // properties
        public String ItemID
        {
            get { return item_id; }
        }

        public String Description
        {
            get { return description; }
            set { description = value; }
        }

        public Dictionary<string, barcode> BarcodeList
        {
            get { return barcodeList; }
        }

        public inventoryItem(String i, String d)
        {
            if (inventoryItem.itemDictionary.ContainsKey(i))
            {
                throw new Exception("inventoryItem(): Cannot Add Duplicate Item ID Key!");
            }
            else
            {
                item_id = i;
                description = d;
                inventoryItem.itemDictionary.Add(item_id, this);
            }
        }

        public inventoryItem(String i)
        {
            if (inventoryItem.itemDictionary.ContainsKey(i))
            {
                throw new Exception("inventoryItem(): Cannot Add Duplicate Item ID Key!");
            }
            else
            {
                item_id = i;
                inventoryItem.itemDictionary.Add(item_id, this);
            }
        }

        public override string ToString()
        {
            int paddingSize = 24;
            string idTxt = item_id.Trim().PadRight(paddingSize);
            return "Item ID: " + idTxt + "Description: " + description;
        }

        public void assignBarcode(String barcodeText)
        {
            if (!barcodeList.ContainsKey(barcodeText))
            {
                barcode b = new barcode(barcodeText, this);
                barcodeList.Add(barcodeText, b);
            }
        }

        public XmlNode encodeXML(XmlDocument doc)
        {
            // The outer node element is an "Item" node.
            XmlNode itemNode = doc.CreateNode("element", "Item", "");

            // It contains at least one child node, the "Item_ID" node
            XmlNode itemIDNode = doc.CreateNode("element", "Item_ID", "");
            itemIDNode.InnerText = this.item_id;
            itemNode.AppendChild(itemIDNode);

            // It can optionally have a description node if the item has a valid description
            if (this.description != null && this.description.Length > 0)
            {
                XmlNode descriptNode = doc.CreateNode("element", "Description", "");
                descriptNode.InnerText = this.description;
                itemNode.AppendChild(descriptNode);
            }

            // if it has any barcodes in its list, then we will add a child node called
            // <Barcodes> with an individual <Barcode> node for each barcode.
            if (this.barcodeList.Count > 0)
            {
                XmlNode barcodesNode = doc.CreateNode("element", "Barcodes", "");
                foreach (KeyValuePair<string, barcode> entry in barcodeList)
                {
                    barcode bc = entry.Value;
                    XmlNode barcodeNode = doc.CreateNode("element", "Barcode", "");
                    barcodeNode.InnerText = bc.getBarCodeText();
                    barcodesNode.AppendChild(barcodeNode);
                }
                itemNode.AppendChild(barcodesNode);
            }
            return itemNode;
        }
    }
}