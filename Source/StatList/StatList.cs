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
        public StatList(IComparer<KeyType> comparer, ICombiner<ValueType> combiner)
        {
            this.keyComparer = comparer;
            this.rootNode = null;
            this.valueCombiner = combiner;
            this.OptimizeForLocality = true;
        }
        // returns the value that is calculated when combining all of the items
        public ValueType CombineAll()
        {
            return this.GetAggregate(this.rootNode);
        }
        public bool OptimizeForLocality { get; set; } // tells whether we should assume that keys are usually (but not necessarily) near each other
        public int NumItems
        {
            get
            {
                return this.numItems;
            }
        }
        // locates the correct spot to put the new value, adds it, and updates any necessary statistics
        public void Add(KeyType newKey, ValueType newValue)
        {
            this.numItems++;
            //this.DebugCheck();
            // create the new node
            TreeNode<KeyType, ValueType> nodeObject = new TreeNode<KeyType, ValueType>(newKey, newValue);
            // special case for the first item
            if (this.rootNode == null)
            {
                this.rootNode = this.latestNode = this.rightmostNode = this.leftmostNode = nodeObject;
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
            // flag that all data for this node (and all of its ancestors) may be out-of-date
            this.InvalidateAggregate(currentNode);
            //bool changed = true;
            // Now go back up the tree and update all the pointers
            /*
            while (currentNode != null)
            {
                this.UpdateFromChildren(currentNode);
                currentNode = currentNode.Parent;
            }
            */
            
            // Now we rebalance the tree
            currentNode = nodeObject;
            TreeNode<KeyType, ValueType> parent = nodeObject.Parent;
            TreeNode<KeyType, ValueType> grandParent = parent.Parent;

            while (grandParent != null)
            {
                TreeNode<KeyType, ValueType> uncle;
                if (parent == grandParent.LeftChild)
                    uncle = grandParent.RightChild;
                else
                    uncle = grandParent.LeftChild;
                //this.DebugCheck();
                // figure out what the depth of the grandparent was the last time it was updated
                int prevGrandparentDepth = grandParent.DepthAtLatestRebalancing;
                // check for an imbalance
                if (this.GetMaxDepth(parent) > this.GetMaxDepth(uncle) + 1)
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
                    parent.DepthAtLatestRebalancing = this.GetMaxDepth(parent);
                    parent.LeftChild.DepthAtLatestRebalancing = this.GetMaxDepth(parent.LeftChild);
                    parent.RightChild.DepthAtLatestRebalancing = this.GetMaxDepth(parent.RightChild);
                    //this.DebugCheck();
                    // decide whether we should stop or whether it's worth checking the next level of the tree
                    // if this branch of the tree is no deeper than it had been, then we don't need to rebalance it any more
                    if (this.GetMaxDepth(parent) < prevGrandparentDepth)
                        break;
                }
                else
                {
                    grandParent.DepthAtLatestRebalancing = this.GetMaxDepth(grandParent);
                    parent.DepthAtLatestRebalancing = this.GetMaxDepth(parent);
                    currentNode.DepthAtLatestRebalancing = this.GetMaxDepth(currentNode);
                    // decide whether we should stop or whether it's worth checking the next level of the tree
                    // if this branch of the tree is no deeper than it had been, then we don't need to rebalance it any more
                    if (this.GetMaxDepth(parent) < prevGrandparentDepth)
                        break;
                }
                // move to the next level of the tree and continue
                currentNode = parent;
                parent = currentNode.Parent;
                if (parent == null)
                    grandParent = null;
                else
                    grandParent = parent.Parent;
            }
            // now we've finally rebalanced the tree and updated all the statistics along the way too
            // all we have to do now is update the rightmost node
            if (this.leftmostNode.LeftChild != null)
                this.leftmostNode = this.leftmostNode.LeftChild;
            if (this.rightmostNode.RightChild != null)
                this.rightmostNode = this.rightmostNode.RightChild;
            //this.latestNode = nodeObject;
            //this.DebugCheck();
        }
        public void Remove(KeyType key)
        {
            this.Remove(key, this.rootNode);
        }
        // removes an item with the given key. It probably removes the one with the maximum index if there are several with the given key
        private void Remove(KeyType key, TreeNode<KeyType, ValueType> startingNode)
        {
            TreeNode<KeyType, ValueType> currentNode = startingNode;
            // find the node indicated by the given key
            while (currentNode != null)
            {
                int comparison = this.keyComparer.Compare(key, currentNode.Key);
                if (comparison == 0)
                {
                    this.Remove(currentNode);
                    return;
                }
                currentNode = this.ChooseChild(key, currentNode);
            }
        }
        // removes the given node
        private void Remove(TreeNode<KeyType, ValueType> nodeToRemove)
        {
            // remove the node and flag that the statistics need updating
            this.InvalidateAggregate(nodeToRemove);
            TreeNode<KeyType, ValueType> newChild = this.MergeChildren(nodeToRemove.LeftChild, nodeToRemove.RightChild);
            this.ReplaceChild(nodeToRemove, newChild);
            this.InvalidateAggregate(newChild);
            this.numItems--;
            //this.DebugCheck();
        }
        // splices the two nodes together into one
        private TreeNode<KeyType, ValueType> MergeChildren(TreeNode<KeyType, ValueType> leftChild, TreeNode<KeyType, ValueType> rightChild)
        {
            if (leftChild == null)
                return rightChild;
            if (rightChild == null)
                return leftChild;
            if (this.GetMaxDepth(leftChild) > this.GetMaxDepth(rightChild))
            {
                // move the left child up
                rightChild.LeftChild = this.MergeChildren(leftChild.RightChild, rightChild.LeftChild);
                leftChild.RightChild = rightChild;
                this.InvalidateAggregate(leftChild);
                this.InvalidateAggregate(rightChild);
                return leftChild;
            }
            else
            {
                // move the right child up
                leftChild.RightChild = this.MergeChildren(leftChild.RightChild, rightChild.LeftChild);
                rightChild.LeftChild = leftChild;
                this.InvalidateAggregate(leftChild);
                this.InvalidateAggregate(rightChild);
                return rightChild;
            }
        }
        public ListItemStats<KeyType, ValueType> FindPreviousItem(KeyType nextKey, bool strictlyLess)
        {
            if (!this.OptimizeForLocality)
                return this.FindPreviousItem(nextKey, strictlyLess, this.rootNode);
            TreeNode<KeyType, ValueType> currentNode = this.latestNode;
            if (currentNode == null)
                return null;
            TreeNode<KeyType, ValueType> parent = currentNode.Parent;
            // loop until we're spanning the input
            while (parent != null)
            {
                if (!this.LowerCanExist(currentNode, nextKey, false))
                    break;
                currentNode = currentNode.Parent;
                parent = currentNode.Parent;
            }
            while (parent != null)
            {
                if (!this.HigherCanExist(currentNode, nextKey, false))
                    break;
                currentNode = currentNode.Parent;
                parent = currentNode.Parent;
            }

            ListItemStats<KeyType, ValueType> result;
            result = this.FindPreviousItem(nextKey, strictlyLess, currentNode);
            //ListItemStats<KeyType, ValueType> correctResult = this.FindPreviousItem(nextKey, strictlyLess, this.rootNode);
            //if (result != correctResult && this.keyComparer.Compare(result.Key, correctResult.Key) != 0)
            //    result = result;
            return result;
        }
        // finds the TreeNode with the largest key less than nextKey
        private ListItemStats<KeyType, ValueType> FindPreviousItem(KeyType nextKey, bool strictlyLess, TreeNode<KeyType, ValueType> startingNode)
        {
            TreeNode<KeyType, ValueType> bestNode = null;
            TreeNode<KeyType, ValueType> currentNode = startingNode;

            while (currentNode != null)
            {
                // remember the last node we visited
                this.latestNode = currentNode;
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
                result = bestNode.Stats;
            }
            return result;
        }

        // finds the TreeNode with the smallest key more than nextKey
        public ListItemStats<KeyType, ValueType> FindNextItem(KeyType nextKey, bool strictlyGreater)
        {
            TreeNode<KeyType, ValueType> bestNode = null;
            TreeNode<KeyType, ValueType> currentNode = this.rootNode;

            while (currentNode != null)
            {
                // choose the child to move to
                if (this.ChooseLeftChild(nextKey, currentNode, !strictlyGreater))
                {
                    // keep track of the rightmost node found so far that is to the left of this one
                    bestNode = currentNode;
                    // move to the correct child
                    currentNode = currentNode.LeftChild;
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
                result = bestNode.Stats;
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
                        return currentNode.Stats;
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
        public ListItemStats<KeyType, ValueType> GetFirstValue()
        {
            if (this.NumItems == 0)
            {
                return null;
            }
            return this.GetValueAtIndex(0);
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
        /*public ValueType CombineBetweenKeys(KeyType left, bool leftInclusive, KeyType right, bool rightInclusive)
        {
            ValueType leftSum = this.CombineThroughKey(left, !leftInclusive);
            ValueType rightSum = this.CombineThroughKey(right, rightInclusive);
            // this call to Difference should be replaced by a bunch of calls to Combine so that it will support functions like Max for which Difference doesn't make sense
            ValueType result = this.valueCombiner.Difference(rightSum, leftSum);
            return result;
        }*/

        public ValueType CombineBetweenKeys(KeyType leftKey, bool leftInclusive, KeyType rightKey, bool rightInclusive)
        {
            if (!this.OptimizeForLocality)
                return this.CombineBetweenKeys(leftKey, leftInclusive, rightKey, rightInclusive, this.rootNode);

            ValueType result;
            //ValueType correctResult;
            // we use the assumption that the node we're looking for is near the latest node we looked at
            TreeNode<KeyType, ValueType> currentNode = this.latestNode;
            if (currentNode == null)
                return valueCombiner.Default();
            TreeNode<KeyType, ValueType> parent = currentNode.Parent;
            //TreeNode<KeyType, ValueType> startingNode = currentNode;

            // walk up the tree until we find a node that certainly contains the key value
            while (parent != null)
            {
                if(!this.LowerCanExist(currentNode, leftKey, !leftInclusive))
                    break;
                currentNode = currentNode.Parent;
                parent = currentNode.Parent;
            }
            while (parent != null)
            {
                if (!this.HigherCanExist(currentNode, rightKey, !rightInclusive))
                    break;
                currentNode = currentNode.Parent;
                parent = currentNode.Parent;
            }

            result = this.CombineBetweenKeys(leftKey, leftInclusive, rightKey, rightInclusive, currentNode);

            //correctResult = this.CombineBetweenKeys(leftKey, leftInclusive, rightKey, rightInclusive, this.rootNode);

            return result;

        }
        private ValueType CombineBetweenKeys(KeyType leftKey, bool leftInclusive, KeyType rightKey, bool rightInclusive, TreeNode<KeyType, ValueType> startingNode)
        {
            /* // make sure the range is not empty
            int keyComparison = this.keyComparer.Compare(rightKey, leftKey);
            if (keyComparison < 0)
            {
                return this.valueCombiner.Default();
            }
            if (keyComparison == 0)
            {
                if (!leftInclusive || !rightInclusive)
                {
                    return this.valueCombiner.Default();
                }
            }*/
            // check that we have data to add up
            if (this.rootNode == null)
            {
                return this.valueCombiner.Default();
            }
            // initialize pointers
            TreeNode<KeyType, ValueType> currentNode = startingNode;
            TreeNode<KeyType, ValueType> leftNode = currentNode;
            TreeNode<KeyType, ValueType> rightNode = currentNode;
            // find the first spot where the paths diverge
            while ((leftNode == rightNode) && (leftNode != null))
            {
                this.latestNode = leftNode;
                currentNode = leftNode;
                leftNode = this.ChooseChild(leftKey, currentNode, leftInclusive);
                rightNode = this.ChooseChild(rightKey, currentNode, !rightInclusive);
            }
            // if the paths didn't split correctly at the end, then the range is empty
            if (!this.ChooseLeftChild(leftKey, currentNode, leftInclusive) || this.ChooseLeftChild(rightKey, currentNode, !rightInclusive))
            {
                return valueCombiner.Default();
            }
            // add up the total
            ValueType aggregate = currentNode.Value;
            if (leftNode != null)
            {
                ValueType leftSum = this.CombineAfterKey(leftKey, leftInclusive, leftNode);
                aggregate = this.valueCombiner.Combine(leftSum, aggregate);
            }
            if (rightNode != null)
            {
                ValueType rightSum = this.CombineBeforeKey(rightKey, rightInclusive, rightNode);
                aggregate = this.valueCombiner.Combine(aggregate, rightSum);
            }
            return aggregate;
        }

        // adds up the values of all elements with keys before this one
        public ValueType CombineBeforeKey(KeyType key, bool inclusive)
        {
            return this.CombineBeforeKey(key, inclusive, this.rootNode);
        }

        // adds up the values of all elements with keys after this one
        public ValueType CombineAfterKey(KeyType key, bool inclusive)
        {
            return this.CombineAfterKey(key, inclusive, this.rootNode);
        }

        public int CountBeforeKey(KeyType key, bool inclusive)
        {
            return this.CountBeforeKey(key, inclusive, this.rootNode);
        }

        public void Clear()
        {
            this.rootNode = this.latestNode = null;
        }
        /*
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
        */
        // This is faster to calculate than DebugList
        public IEnumerable<ListItemStats<KeyType, ValueType>> AllItems
        {
            get
            {
                LinkedList<ListItemStats<KeyType, ValueType>> results = new LinkedList<ListItemStats<KeyType, ValueType>>();
                if (this.rootNode != null)
                    this.GetAllItems(this.rootNode, results);
                return results;
            }
        }
        public LinkedList<ListItemStats<KeyType, ValueType>> ItemsFromIndex(int indexInclusive)
        {
            // this hasn't been tested yet
            LinkedList<ListItemStats<KeyType, ValueType>> resultList = new LinkedList<ListItemStats<KeyType, ValueType>>();
            this.GetItemsFromIndex(indexInclusive, this.rootNode, resultList);
            return resultList;
        }

        #endregion

        #region Private Member Functions

        private void GetItemsFromIndex(int indexInclusive, TreeNode<KeyType, ValueType> startingNode, LinkedList<ListItemStats<KeyType, ValueType>> outputList)
        {
            // this function hasn't been tested yet
            int leftCount = this.GetSubnodeCount(startingNode.LeftChild);
            if (indexInclusive < leftCount)
            {
                // if we get here, then the first node is in the left child
                if (startingNode.LeftChild != null)
                {
                    this.GetItemsFromIndex(indexInclusive, startingNode.LeftChild, outputList);
                }
                outputList.AddLast(startingNode.Stats);
                if (startingNode.RightChild != null)
                    this.GetAllItems(startingNode.RightChild, outputList);
            }
            else
            {
                if (indexInclusive == leftCount)
                {
                    // if we get here, then the first node is here
                    outputList.AddLast(startingNode.Stats);
                    if (startingNode.RightChild != null)
                        this.GetAllItems(startingNode.RightChild, outputList);
                }
                else
                {
                    // if we get here, then the first node is in the right child
                    if (startingNode.RightChild != null)
                        this.GetItemsFromIndex(indexInclusive - (leftCount + 1), startingNode.RightChild, outputList);
                }
            }
        }

        // returns a list of all of the items in the StatList
        private void GetAllItems(TreeNode<KeyType, ValueType> startingNode, LinkedList<ListItemStats<KeyType, ValueType>> outputList)
        {
            if (startingNode.LeftChild != null)
                this.GetAllItems(startingNode.LeftChild, outputList);
            //outputList.Add(startingNode.Value);
            outputList.AddLast(startingNode.Stats);

            if (startingNode.RightChild != null)
                this.GetAllItems(startingNode.RightChild, outputList);
        }

        // adds up the values of all elements with keys before this one
        private ValueType CombineBeforeKey(KeyType key, bool inclusive, TreeNode<KeyType, ValueType> startingNode)
        {
            TreeNode<KeyType, ValueType> currentNode = startingNode;
            ValueType aggregate = this.valueCombiner.Default();
            while (currentNode != null)
            {
                if (this.ChooseLeftChild(key, currentNode, !inclusive))
                {
                    currentNode = currentNode.LeftChild;
                }
                else
                {
                    ValueType extraValue = this.valueCombiner.Combine(this.GetAggregate(currentNode.LeftChild), currentNode.Value);
                    aggregate = this.valueCombiner.Combine(aggregate, extraValue);
                    currentNode = currentNode.RightChild;
                }
            }
            return aggregate;
        }

        // adds up the values of all elements with keys after this one
        private ValueType CombineAfterKey(KeyType key, bool inclusive, TreeNode<KeyType, ValueType> startingNode)
        {
            TreeNode<KeyType, ValueType> currentNode = startingNode;
            ValueType Combine = this.valueCombiner.Default();
            while (currentNode != null)
            {
                if (this.ChooseLeftChild(key, currentNode, inclusive))
                {
                    ValueType extraValue = this.valueCombiner.Combine(currentNode.Value, this.GetAggregate(currentNode.RightChild));
                    Combine = this.valueCombiner.Combine(extraValue, Combine);
                    currentNode = currentNode.LeftChild;
                }
                else
                {
                    currentNode = currentNode.RightChild;
                }
            }
            return Combine;
        }

        // Updates the data for the given node, and returns true if and only if something changed
        private void UpdateFromChildren(TreeNode<KeyType, ValueType> node)
        {
            // update node.Aggregate
            ValueType leftSum = this.GetAggregate(node.LeftChild);
            ValueType middle = node.Value;
            ValueType rightSum = this.GetAggregate(node.RightChild);
            node.Aggregate = this.valueCombiner.Combine(this.valueCombiner.Combine(leftSum, middle), rightSum);

            // update node.SubnodeCount
            int previousCount = node.SubnodeCount;
            int newCount = this.GetSubnodeCount(node.LeftChild) + 1 + this.GetSubnodeCount(node.RightChild);
            node.SubnodeCount = newCount;

            // update node.MaxDepth
            node.MaxDepth = Math.Max(this.GetMaxDepth(node.LeftChild), this.GetMaxDepth(node.RightChild)) + 1;

            // update node.MinSubkey and node.MaxSubkey
            if (node.LeftChild != null)
                node.MinSubkey = node.LeftChild.MinSubkey;
            else
                node.MinSubkey = node.Key;
            if (node.RightChild != null)
                node.MaxSubkey = node.RightChild.MaxSubkey;
            else
                node.MaxSubkey = node.Key;

            // flag that it is up-to-date
            node.Updated = true;
        }
        private int GetSubnodeCount(TreeNode<KeyType, ValueType> node)
        {
            if (node == null)
                return 0;
            if (!node.Updated)
                this.UpdateFromChildren(node);
            return node.SubnodeCount;
        }
        private ValueType GetAggregate(TreeNode<KeyType, ValueType> node)
        {
            if (node == null)
                return valueCombiner.Default();
            if (!node.Updated)
                this.UpdateFromChildren(node);
            return node.Aggregate;
        }
        private int GetMaxDepth(TreeNode<KeyType, ValueType> node)
        {
            if (node == null)
                return 0;
            if (!node.Updated)
                this.UpdateFromChildren(node);
            return node.MaxDepth;
        }
        private KeyType GetMinSubkey(TreeNode<KeyType, ValueType> node)
        {
            if (!node.Updated)
                this.UpdateFromChildren(node);
            return node.MinSubkey;
        }
        private KeyType GetMaxSubkey(TreeNode<KeyType, ValueType> node)
        {
            if (!node.Updated)
                this.UpdateFromChildren(node);
            return node.MaxSubkey;
        }
        // flags as invalid all aggregate data for this node and all of its parents
        private void InvalidateAggregate(TreeNode<KeyType, ValueType> node)
        {
            TreeNode<KeyType, ValueType> currentNode = node;
            while (currentNode != null)
            {
                // if it's already been invalidated, then we don't need to bother invalidating it or any ancestors again
                if (currentNode.Updated == false)
                    break;
                // invalidate this node
                currentNode.Updated = false;
                // move to the parent
                currentNode = currentNode.Parent;
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
            TreeNode<KeyType, ValueType> result = null;
            if (this.ChooseLeftChild(key, this.leftmostNode))
                result = this.FindLeaf(key, this.leftmostNode);
            if (!this.ChooseLeftChild(key, this.rightmostNode))
                result = this.rightmostNode;
            //TreeNode<KeyType, ValueType> correctResult;
            if (result != null)
            {
                /*
                correctResult = this.FindLeaf(key, this.rootNode);

                if (this.keyComparer.Compare(result.Key, correctResult.Key) != 0)
                    result = result;
                */
                return result;
            }

            if (!this.OptimizeForLocality)
                return this.FindLeaf(key, this.rootNode);


            // we use the assumption that the node we're looking for is near the latest node we looked at
            TreeNode<KeyType, ValueType> currentNode = this.latestNode;
            TreeNode<KeyType, ValueType> parent = currentNode.Parent;
            //TreeNode<KeyType, ValueType> startingNode = currentNode;

            // walk up the tree until we find a node that certainly contains the key value
            while (parent != null) 
            {
                if (this.keyComparer.Compare(key, this.GetMinSubkey(currentNode)) > 0 &&
                    this.keyComparer.Compare(key, this.GetMaxSubkey(currentNode)) < 0)
                    break;
                currentNode = parent;
                parent = currentNode.Parent;
            }
            result = this.FindLeaf(key, currentNode);
            
            /*
            correctResult = this.FindLeaf(key, this.rootNode);

            if (this.keyComparer.Compare(result.Key, correctResult.Key) != 0)
                result = result;
            */
            return result;
        }
        private TreeNode<KeyType, ValueType> FindLeaf(KeyType key, TreeNode<KeyType, ValueType> startingNode)
        {
            // first we check whether this key is after our maximum key, because they are often provided in sorted order
            TreeNode<KeyType, ValueType> currentNode = startingNode;
            TreeNode<KeyType, ValueType> newNode = currentNode;
            while (newNode != null)
            {
                currentNode = newNode;
                newNode = this.ChooseChild(key, currentNode);
            }
            this.latestNode = currentNode;
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
                if (!this.CheckNode(this.rootNode))
                    throw new Exception("debug check failed");
                return false;
            }
            return true;
        }
        private bool CheckNode(TreeNode<KeyType, ValueType> node)
        {
            if (node.LeftChild != null)
            {
                if (node.LeftChild.Parent != node)
                    return false;
                if (!this.CheckNode(node.LeftChild))
                    return false;
                if (this.keyComparer.Compare(node.LeftChild.Key, node.Key) > 0)
                    return false;
            }
            if (node.RightChild != null)
            {
                if (node.RightChild.Parent != node)
                    return false;
                if (!this.CheckNode(node.RightChild))
                    return false;
                if (keyComparer.Compare(node.Key, node.RightChild.Key) > 0)
                    return false;
            }
            if (node.Updated)
            {
                if (node.LeftChild != null && node.LeftChild.Updated == false)
                    return false;
                if (node.RightChild != null && node.RightChild.Updated == false)
                    return false;
                if (node.SubnodeCount != (1 + this.GetSubnodeCount(node.LeftChild) + this.GetSubnodeCount(node.RightChild)))
                    return false;
            }
            if (Math.Abs(this.GetMaxDepth(node.LeftChild) - this.GetMaxDepth(node.RightChild)) > 1)
                return false;
            
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
        // returns true if there may exist another node to the left of node, with key more than minKey
        private bool LowerCanExist(TreeNode<KeyType, ValueType> node, KeyType minKey, bool strictlyGreater)
        {
            if (this.keyComparer.Compare(this.GetMinSubkey(node), this.leftmostNode.Key) == 0)
                return false;
            if (strictlyGreater)
                return (this.keyComparer.Compare(minKey, this.GetMinSubkey(node)) < 0);
            else
                return (this.keyComparer.Compare(minKey, this.GetMinSubkey(node)) <= 0);
        }
        // returns true if there may exist another node to the right of node, with key less than maxKey
        private bool HigherCanExist(TreeNode<KeyType, ValueType> node, KeyType maxKey, bool strictlyLess)
        {
            if (this.keyComparer.Compare(this.GetMaxSubkey(node), this.rightmostNode.Key) == 0)
                return false;
            if (strictlyLess)
                return (this.keyComparer.Compare(maxKey, this.GetMaxSubkey(node)) > 0);
            else
                return (this.keyComparer.Compare(maxKey, this.GetMaxSubkey(node)) >= 0);
        }


        #endregion

        private TreeNode<KeyType, ValueType> rootNode;
        private TreeNode<KeyType, ValueType> latestNode;
        private TreeNode<KeyType, ValueType> rightmostNode;
        private TreeNode<KeyType, ValueType> leftmostNode;

        private IComparer<KeyType> keyComparer;
        private ICombiner<ValueType> valueCombiner;
        private int numItems;
    }
}
