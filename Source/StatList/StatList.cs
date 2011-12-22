using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatLists
{
    public class StatList<KeyType, ValueType>
    {
        #region Public Member Functions

        // constructor
        public StatList(IComparer<KeyType> comparer, IAdder<ValueType> adder)
        {
            this.keyComparer = comparer;
            this.rootNode = null;
            this.valueAdder = adder;
        }

        public int NumItems
        {
            get
            {
                if (this.rootNode != null)
                {
                    return this.rootNode.SubnodeCount;
                }
                else
                {
                    return 0;
                }
            }
        }
        // locates the correct spot to put the new value, adds it, and updates any necessary statistics
        public void Add(KeyType newKey, ValueType newValue)
        {
            //this.DebugCheck();
            // create the new node
            TreeNode<KeyType, ValueType> nodeObject = new TreeNode<KeyType, ValueType>(newKey, newValue);
            // special case for the first item
            if (this.rootNode == null)
            {
                this.rootNode = nodeObject;
                return;
            }
            // find the item to add to
            TreeNode<KeyType, ValueType> currentNode = this.FindLeaf(newKey);
            // add the item
            if (this.ChooseLeftChild(newKey, currentNode))
            {
                currentNode.LeftChild = nodeObject;
            }
            else
            {
                currentNode.RightChild = nodeObject;
            }
            // Now go back up the tree and update all the pointers
            while (currentNode != null)
            {
                this.UpdateFromChildren(currentNode);
                currentNode = currentNode.Parent;
            }

            // Now we rebalance the tree
            currentNode = nodeObject;
            TreeNode<KeyType, ValueType> parent = nodeObject.Parent;
            TreeNode<KeyType, ValueType> grandParent = parent.Parent;

            while (grandParent != null)
            {
                //this.DebugCheck();
                // check for an imbalance
                if (grandParent.SubnodeCount * 2 <= parent.SubnodeCount * 3)
                {
                    bool chooseLeft1 = (grandParent.LeftChild == parent);
                    bool chooseLeft2 = (parent.LeftChild == currentNode);
                    TreeNode<KeyType, ValueType> greatGrandparent = grandParent.Parent;
                    if (chooseLeft1)
                    {
                        if (chooseLeft2)
                        {
                            // __*
                            // _*_
                            // *__
                            // shuffle the pointers around to rebalance the tree
                            TreeNode<KeyType, ValueType> temp = parent.RightChild;
                            // find whichever node had grandParent as a child, and replace it with parent
                            this.ReplaceChild(grandParent, parent);
                            parent.RightChild = grandParent;
                            grandParent.LeftChild = temp;
                        }
                        else
                        {
                            // __*
                            // *_
                            // _*_
                            // shuffle the pointers around to rebalance the tree
                            TreeNode<KeyType, ValueType> tempLeft = currentNode.LeftChild;
                            TreeNode<KeyType, ValueType> tempRight = currentNode.RightChild;
                            // find whichever node had grandParent as a child, and replace it with currentNode
                            this.ReplaceChild(grandParent, currentNode);
                            currentNode.LeftChild = parent;
                            currentNode.RightChild = grandParent;
                            parent.RightChild = tempLeft;
                            grandParent.LeftChild = tempRight;
                            // make sure parent points to the actual parent so we can continue smoothly in further iterations
                            parent = currentNode;
                        }
                    }
                    else
                    {
                        if (chooseLeft2)
                        {
                            // *__
                            // __*
                            // _*_
                            TreeNode<KeyType, ValueType> tempLeft = currentNode.LeftChild;
                            TreeNode<KeyType, ValueType> tempRight = currentNode.RightChild;
                            // find whichever node had grandParent as a child, and replace it with currentNode
                            this.ReplaceChild(grandParent, currentNode);
                            currentNode.LeftChild = grandParent;
                            currentNode.RightChild = parent;
                            grandParent.RightChild = tempLeft;
                            parent.LeftChild = tempRight;
                            // make sure parent points to the actual parent so we can continue smoothly in further iterations
                            parent = currentNode;
                        }
                        else
                        {
                            // *__
                            // _*_
                            // __*
                            // shuffle the pointers around to rebalance the tree
                            TreeNode<KeyType, ValueType> temp = parent.LeftChild;
                            // find whichever node had grandParent as a child, and replace it with parent
                            this.ReplaceChild(grandParent, parent);
                            parent.LeftChild = grandParent;
                            grandParent.RightChild = temp;
                        }
                    }
                    // update the statistics
                    this.UpdateFromChildren(parent.LeftChild);
                    this.UpdateFromChildren(parent.RightChild);
                    this.UpdateFromChildren(parent);
                    this.DebugCheck();
                }
                // move to the next level of the tree and continue
                currentNode = parent;
                parent = currentNode.Parent;
                if (parent == null)
                    grandParent = null;
                else
                    grandParent = parent.Parent;
            }
            // now we've finally rebalanced the tree and updated all the statistics along the way too, so we're done
        }

        // finds the TreeNode with the largest key less than nextKey
        public ListItemStats<KeyType, ValueType> FindPreviousItem(KeyType nextKey, bool strictlyLess)
        {
            TreeNode<KeyType, ValueType> bestNode = null;
            TreeNode<KeyType, ValueType> currentNode = this.rootNode;

            while (currentNode != null)
            {
                // choose the child to move to
                if (this.ChooseLeftChild(nextKey, currentNode, strictlyLess))
                {
                    // move to the correct child
                    currentNode = currentNode.LeftChild;
                }
                else
                {
                    // keep track of the number of items before this one, including itself
                    //lowerCount += currentNode.GetNumLeftChildren() + 1;
                    // keep track of the rightmost node found so far that is to the left of this one
                    bestNode = currentNode;
                    // move to the correct child
                    currentNode = currentNode.RightChild;
                }
            }
            ListItemStats<KeyType, ValueType> result = null;
            if (bestNode != null)
            {
                result = new ListItemStats<KeyType, ValueType>(bestNode.Key, bestNode.Value);
            }
            return result;
        }

        // finds the TreeNode with the smallest key less than nextKey
        public ListItemStats<KeyType, ValueType> FindNextItem(KeyType nextKey, bool strictlyGreater)
        {
            TreeNode<KeyType, ValueType> bestNode = null;
            TreeNode<KeyType, ValueType> currentNode = this.rootNode;

            while (currentNode != null)
            {
                // choose the child to move to
                if (this.ChooseLeftChild(nextKey, currentNode, !strictlyGreater))
                {
                    // move to the correct child
                    currentNode = currentNode.LeftChild;
                    // keep track of the rightmost node found so far that is to the left of this one
                    bestNode = currentNode;
                }
                else
                {
                    // move to the correct child
                    currentNode = currentNode.RightChild;
                }
            }
            ListItemStats<KeyType, ValueType> result = null;
            if (bestNode != null)
            {
                result = new ListItemStats<KeyType, ValueType>(bestNode.Key, bestNode.Value);
            }
            return result;
        }

        public ListItemStats<KeyType, ValueType> GetValueAtIndex(int index)
        {
            TreeNode<KeyType, ValueType> currentNode = this.rootNode;
            int totalLowerCount = 0;
            while (currentNode != null)
            {
                int currentLowerCount = currentNode.GetNumLeftChildren();
                if (index < totalLowerCount + currentLowerCount)
                {
                    currentNode = currentNode.LeftChild;
                }
                else
                {
                    if (index == totalLowerCount + currentLowerCount)
                    {
                        // found it
                        return new ListItemStats<KeyType, ValueType>(currentNode.Key, currentNode.Value);
                    }
                    else
                    {
                        totalLowerCount += currentLowerCount + 1;
                        currentNode = currentNode.RightChild;
                    }
                }
            }
            // index out of bounds error
            return null;
        }
        public ListItemStats<KeyType, ValueType> GetLastValue()
        {
            if (this.NumItems == 0)
            {
                return null;
            }
            return this.GetValueAtIndex(this.NumItems - 1);
        }

        /*public int IndexOfPreviousItem(KeyType nextKey, bool strictlyLess)
        {
            ListItemStats<KeyType, ValueType> itemStats = this.FindPreviousItem(nextKey, strictlyLess);
            if (itemStats == null)
            {
                return -1;
            }
            else
            {
                return itemStats.Index;
            }
        }*/
        /*public ValueType SumBetweenKeys(KeyType left, bool leftInclusive, KeyType right, bool rightInclusive)
        {
            ValueType leftSum = this.SumThroughKey(left, !leftInclusive);
            ValueType rightSum = this.SumThroughKey(right, rightInclusive);
            // this call to Difference should be replaced by a bunch of calls to Sum so that it will support functions like Max for which Difference doesn't make sense
            ValueType result = this.valueAdder.Difference(rightSum, leftSum);
            return result;
        }*/

        public ValueType SumBetweenKeys(KeyType leftKey, bool leftInclusive, KeyType rightKey, bool rightInclusive)
        {
            /* // make sure the range is not empty
            int keyComparison = this.keyComparer.Compare(rightKey, leftKey);
            if (keyComparison < 0)
            {
                return this.valueAdder.Zero();
            }
            if (keyComparison == 0)
            {
                if (!leftInclusive || !rightInclusive)
                {
                    return this.valueAdder.Zero();
                }
            }*/
            // check that we have data to add up
            if (this.rootNode == null)
            {
                return this.valueAdder.Zero();
            }
            // initialize pointers
            TreeNode<KeyType, ValueType> currentNode = this.rootNode;
            TreeNode<KeyType, ValueType> leftNode = this.rootNode;
            TreeNode<KeyType, ValueType> rightNode = this.rootNode;
            // find the first spot where the paths diverge
            while ((leftNode == rightNode) && (leftNode != null))
            {
                currentNode = leftNode;
                leftNode = this.ChooseChild(leftKey, currentNode, leftInclusive);
                rightNode = this.ChooseChild(rightKey, currentNode, !rightInclusive);
            }
            // if the paths didn't split correctly at the end, then the range is empty
            if (!this.ChooseLeftChild(leftKey, currentNode, leftInclusive) || this.ChooseLeftChild(rightKey, currentNode, !rightInclusive))
            {
                return valueAdder.Zero();
            }
            // add up the total
            ValueType sum = currentNode.Value;
            if (leftNode != null)
            {
                ValueType leftSum = this.SumAfterKey(leftKey, leftInclusive, leftNode);
                sum = this.valueAdder.Sum(sum, leftSum);
            }
            if (rightNode != null)
            {
                ValueType rightSum = this.SumBeforeKey(rightKey, rightInclusive, rightNode);
                sum = this.valueAdder.Sum(sum, rightSum);
            }
            return sum;
        }

        // adds up the values of all elements with keys before this one
        public ValueType SumBeforeKey(KeyType key, bool inclusive)
        {
            return this.SumBeforeKey(key, inclusive, this.rootNode);
        }

        // adds up the values of all elements with keys after this one
        public ValueType SumAfterKey(KeyType key, bool inclusive)
        {
            return this.SumAfterKey(key, inclusive, this.rootNode);
        }

        public int CountBeforeKey(KeyType key, bool inclusive)
        {
            return this.CountBeforeKey(key, inclusive, this.rootNode);
        }

        public void Clear()
        {
            this.rootNode = null;
        }
        public List<ListItemStats<KeyType, ValueType>> DebugList
        {
            get
            {
                List<ListItemStats<KeyType, ValueType>> results = new List<ListItemStats<KeyType, ValueType>>();
                int i;
                for (i = 0; i < this.NumItems; i++)
                {
                    results.Add(this.GetValueAtIndex(i));
                }
                return results;
            }
        }
        // Eventually this should be optimized. For now it's only slow by a factor of log(n) so it's not a big deal
        public List<ListItemStats<KeyType, ValueType>> AllItems
        {
            get
            {
                return this.DebugList;
            }
        }
        #endregion

        #region Private Member Functions

        // adds up the values of all elements with keys before this one
        private ValueType SumBeforeKey(KeyType key, bool inclusive, TreeNode<KeyType, ValueType> startingNode)
        {
            TreeNode<KeyType, ValueType> currentNode = startingNode;
            ValueType sum = this.valueAdder.Zero();
            while (currentNode != null)
            {
                if (this.ChooseLeftChild(key, currentNode, !inclusive))
                {
                    currentNode = currentNode.LeftChild;
                }
                else
                {
                    ValueType extraValue = this.valueAdder.Sum(this.GetLeftSum(currentNode), currentNode.Value);
                    sum = this.valueAdder.Sum(sum, extraValue);
                    currentNode = currentNode.RightChild;
                }
            }
            return sum;
        }

        // adds up the values of all elements with keys after this one
        private ValueType SumAfterKey(KeyType key, bool inclusive, TreeNode<KeyType, ValueType> startingNode)
        {
            TreeNode<KeyType, ValueType> currentNode = startingNode;
            ValueType sum = this.valueAdder.Zero();
            while (currentNode != null)
            {
                if (this.ChooseLeftChild(key, currentNode, inclusive))
                {
                    ValueType extraValue = this.valueAdder.Sum(this.GetRightSum(currentNode), currentNode.Value);
                    sum = this.valueAdder.Sum(sum, extraValue);
                    currentNode = currentNode.LeftChild;
                }
                else
                {
                    currentNode = currentNode.RightChild;
                }
            }
            return sum;
        }

        private void UpdateFromChildren(TreeNode<KeyType, ValueType> node)
        {
            ValueType leftSum = this.GetLeftSum(node);
            ValueType middle = node.Value;
            ValueType rightSum = this.GetRightSum(node);
            node.Aggregate = this.valueAdder.Sum(this.valueAdder.Sum(leftSum, middle), rightSum);

            node.SubnodeCount = node.GetNumLeftChildren() + 1 + node.GetNumRightChildren();

        }
        private ValueType GetLeftSum(TreeNode<KeyType, ValueType> node)
        {
            if (node.LeftChild != null)
            {
                return node.LeftChild.Aggregate;
            }
            else
            {
                return this.valueAdder.Zero();
            }
        }
        private ValueType GetRightSum(TreeNode<KeyType, ValueType> node)
        {
            if (node.RightChild != null)
            {
                return node.RightChild.Aggregate;
            }
            else
            {
                return this.valueAdder.Zero();
            }
        }

        private TreeNode<KeyType, ValueType> GetRightmostItem(TreeNode<KeyType, ValueType> startingNode)
        {
            TreeNode<KeyType, ValueType> node = startingNode;
            while (node.RightChild != null)
            {
                node = node.RightChild;
            }
            return node;
        }

        private int CountBeforeKey(KeyType key, bool inclusive, TreeNode<KeyType, ValueType> startingNode)
        {
            int lowerCount = 0;
            TreeNode<KeyType, ValueType> currentNode = startingNode;
            while (currentNode != null)
            {
                if (this.ChooseLeftChild(key, currentNode, !inclusive))
                {
                    currentNode = currentNode.LeftChild;
                }
                else
                {
                    lowerCount += currentNode.GetNumLeftChildren() + 1;
                    currentNode = currentNode.RightChild;
                }
            }
            return lowerCount;
        }
        // locates the immediate parent of the desired key
        private TreeNode<KeyType, ValueType> FindLeaf(KeyType key)
        {
            TreeNode<KeyType, ValueType> currentNode = this.rootNode;
            TreeNode<KeyType, ValueType> newNode = currentNode;
            while (newNode != null)
            {
                currentNode = newNode;
                newNode = this.ChooseChild(key, currentNode);
            }
            return currentNode;
        }
        // finds the node that had oldChild as a child, and puts newChild in its place
        private void ReplaceChild(TreeNode<KeyType, ValueType> oldChild, TreeNode<KeyType, ValueType> newChild)
        {
            if (oldChild == this.rootNode)
            {
                this.rootNode = newChild;
            }
            else
            {
                TreeNode<KeyType, ValueType> parent = oldChild.Parent;
                parent.ReplaceChild(oldChild, newChild);
            }
        }
        private bool DebugCheck()
        {
            if (this.rootNode != null)
            {
                return this.CheckNode(this.rootNode);
            }
            return true;
        }
        private bool CheckNode(TreeNode<KeyType, ValueType> node)
        {
            if (node.LeftChild != null)
            {
                if (node.LeftChild.Parent != node)
                {
                    return false;
                }
                if (!this.CheckNode(node.LeftChild))
                {
                    return false;
                }
            }
            if (node.RightChild != null)
            {
                if (node.RightChild.Parent != node)
                {
                    return false;
                }
                if (!this.CheckNode(node.RightChild))
                {
                    return false;
                }
            }
            
            return true;
        }
        // tells whether the key belongs to the left child
        private bool ChooseLeftChild(KeyType newKey, TreeNode<KeyType, ValueType> node)
        {
            return this.ChooseLeftChild(newKey, node, false);
        }
        // tells whether to choose the left or right child, and allows for a default choice when the current node's key equals newKey
        private bool ChooseLeftChild(KeyType newKey, TreeNode<KeyType, ValueType> node, bool defaultLeft)
        {
            KeyType nodeKey = node.Key;
            if (defaultLeft)
            {
                if (this.keyComparer.Compare(newKey, nodeKey) <= 0)
                {
                    return true;
                }
            }
            else
            {
                if (this.keyComparer.Compare(newKey, nodeKey) < 0)
                {
                    return true;
                }
            }
            return false;
        }
        // tells which child the key belongs to
        private TreeNode<KeyType, ValueType> ChooseChild(KeyType newKey, TreeNode<KeyType, ValueType> node)
        {
            if (this.ChooseLeftChild(newKey, node))
            {
                return node.LeftChild;
            }
            else
            {
                return node.RightChild;
            }
        }
        // tells which child the key belongs to
        private TreeNode<KeyType, ValueType> ChooseChild(KeyType newKey, TreeNode<KeyType, ValueType> node, bool defaultLeft)
        {
            if (this.ChooseLeftChild(newKey, node, defaultLeft))
            {
                return node.LeftChild;
            }
            else
            {
                return node.RightChild;
            }
        }

        #endregion

        private TreeNode<KeyType, ValueType> rootNode;
        private IComparer<KeyType> keyComparer;
        private IAdder<ValueType> valueAdder;
    }
}
