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


namespace scanToolClasses
{
    public class inventoryItem
    {
        // this static list of inventory items is a linked list of all the inventory items we
        // know about.  We implement it as a class-level linked list so we will always be able
        // to search the list when we need to in order to find an item.
        static public LinkedList<inventoryItem> itemList = new LinkedList<inventoryItem>();

        // members
        private String item_id;
        private String description;

        public String ItemID
        {
            get { return item_id; }
        }

        public String Description
        {
            get { return description; }
            set { description = value; }
        }

        private LinkedList<barcode> barcodeList = new LinkedList<barcode>();

        public inventoryItem(String i, String d)
        {
            item_id = i;
            description = d;
            inventoryItem.itemList.AddLast(this);
        }

        public inventoryItem(String i)
        {
            item_id = i;
            inventoryItem.itemList.AddLast(this);
        }

        public override string ToString()
        {
            return "Item ID: " + item_id + ", Decription: " + description;
        }

        public void assignBarcode(String barcodeText)
        {
            barcode b = new barcode(barcodeText, this);
            barcodeList.AddLast(b);
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
                foreach (barcode bc in barcodeList)
                {
                    XmlNode barcodeNode = doc.CreateNode("element", "Barcode", "");
                    barcodeNode.InnerText = bc.getBarCodeText();
                    barcodesNode.AppendChild(barcodeNode);
                }
                itemNode.AppendChild(barcodesNode);
            }
            return itemNode;
        }


        // Given an xml node named "Item" with all its child nodes, this
        // method creates an inventory item and returns a reference for it
        // to the calling program.  If for some reason, the item cannot be
        // created, most likely because the xml is incorrect, then this
        // method returns a null reference.  The Item node must contain an
        // item_id child node, and it can optionally contain a Description
        // child node.
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
                if (descriptText != null)
                {
                    itm = new inventoryItem(itemText, descriptText);
                }
                else
                {
                    itm = new inventoryItem(itemText);
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

        public static int LoadItems(String configFile)
        {
            int itemsLoaded = 0;
            // first, let's open the file
            try
            {
                XmlDocument itemFile = new XmlDocument();
                itemFile.Load(configFile);

                // now that the file is open, let's parse the xml in it.
                // the file will be full of item nodes, with each item node having
                // a set of child nodes.  Child nodes are item ids, descriptions,
                // and barcodes
                XmlNodeList itms = itemFile.GetElementsByTagName("Item");
                for (int i = 0; i < itms.Count; i++)
                {
                    inventoryItem itm = createItemFromXML(itms[i]);
                    itemsLoaded++;
                    XmlNode bcList = itms[i].SelectSingleNode("Barcodes");
                    assignBarcodesToItemFromXML(bcList, itm);
                }
                //foreach (XmlNode itemNode in itemFile.DocumentElement.ChildNodes)
                //{
                //  inventoryItem itm = createItemFromXML(itemNode);
                //if (itm.Equals(null))
                //    {
                // then things are screwed up and we can't go any further.
                // we do nothing and try to process the next item.
                //    }    
                //   else
                //    {
//            }
//                }
            }
            catch (Exception e)
            {
                // do nothing for right now
            }
            return itemsLoaded;
        }

        public static int saveItems(String configFile)
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
            foreach (inventoryItem itm in inventoryItem.itemList)
            {
                XmlNode itmXml = itm.encodeXML(doc);
                rootNode.AppendChild(itmXml);
                itemsSaved++;
            }
            // now save the file
            doc.Save(configFile);

            return itemsSaved;
        }
    }

    

}