/*
Class Name:  barcode
Description:  Barcode
   A barcode is a value that is scanned into the system using a handheld scanner.  They are
   the UPC barcodes on packages.  A barcode is assigned to an inventory item.
Purpose:  This class is used to associate a UPC barcode with an inventory item

Theory of Operation:

Date:  08/19/2015
Programmer:  Russell Mace 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scanToolClasses
{
    public class barcode
    {
        /* This barcode list is a class variable - there will be only one barcode list.
           It will hold a list of all the barcodes that the system knows about at any one time.
           This will give us a way of looking up to see if the barcode already exists when
           it is scanned.  We implement this as a keyed Dictionary using the C# library's Dictionary
           template class.
        */
        static private Dictionary<string, barcode> barcodeList = new Dictionary<string, barcode>();
        // Declaration of members

        public Dictionary<string, barcode> BarcodeList
        {
            get { return barcodeList; }
        }

        String barcodeText;
        inventoryItem item;

        //Declaration of methods

        // The constructor - we need to have both the barcode string and its associated inventory item
        // when we construct it.  It doesn't make any sense in our system to have a barcode without an
        // associated item.  The only approved constructor of a barcode is an inventory
        // item.  Inventory items create barcodes.  No one else should.
        public barcode(String bc, inventoryItem i)
        {
            barcodeText = bc;
            item = i;
            barcodeList.Add(bc, this);  // this is a new barcode, so we add it to our barcode list
        }

        public inventoryItem getItem() { return item; }
        public String getItemText() { return item.ToString(); }
        public String getBarCodeText() { return this.barcodeText; }

        // we're going to want to search the list of barcodes to see if we already know it in our system.
        // if we do, then we return a reference to the barcode that we found.
        // if not, then we return a null reference
        public static barcode findBarcode(String bc)
        {
            barcode bcfound = null;
            foreach (KeyValuePair<string, barcode> entry in barcodeList)
            {
                string bcode = entry.Key;
                if (bcode == bc)
                {
                    bcfound = entry.Value;
                }
            }
            return bcfound;
        }

        public override string ToString()
        {
            return barcodeText;
        }
    }
}
