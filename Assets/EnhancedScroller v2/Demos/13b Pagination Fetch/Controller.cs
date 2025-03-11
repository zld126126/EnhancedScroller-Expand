using UnityEngine;
using System.Collections;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using System;

namespace EnhancedScrollerDemos.PaginationFetch
{
    /// <summary>
    /// This demo shows how you can load small chunks of data when the user reaches the end of the scroller
    /// </summary>
    public class Controller : MonoBehaviour, IEnhancedScrollerDelegate
    {
        /// <summary>
        /// Internal representation of our data. Note that the scroller will never see
        /// this, so it separates the data from the layout using MVC principles.
        /// </summary>
        private SmallList<Data> _data;

        /// <summary>
        /// This is our scroller we will be a delegate for
        /// </summary>
        public EnhancedScroller scroller;

        /// <summary>
        /// This will be the prefab of each cell in our scroller. Note that you can use more
        /// than one kind of cell, but this example just has the one type.
        /// </summary>
        public CellView cellViewPrefab;

        /// <summary>
        /// Loading cell view prefab displayed at the bottom of the scroller
        /// </summary>
        public LoadingCellView loadingCellViewPrefab;

        /// <summary>
        /// Height of the cells
        /// </summary>
        public int cellHeight;

        /// <summary>
        /// The number of elements per page
        /// </summary>
        public int pageCount;

        /// <summary>
        /// Used to determine if the scroller is already loading new data.
        /// If so, then we don't want to call again to avoid an infinite loop.
        /// </summary>
        private bool _loadingNew;

        /// <summary>
        /// Be sure to set up your references to the scroller after the Awake function. The 
        /// scroller does some internal configuration in its own Awake function. If you need to
        /// do this in the Awake function, you can set up the script order through the Unity editor.
        /// In this case, be sure to set the EnhancedScroller's script before your delegate.
        /// 
        /// In this example, we are calling our initializations in the delegate's Start function,
        /// but it could have been done later, perhaps in the Update function.
        /// </summary>
        void Start()
        {
            // tell the scroller that this script will be its delegate
            scroller.Delegate = this;
            scroller.scrollerScrolled = ScrollerScrolled;

            // initialize the data
            _data = new SmallList<Data>();

            // load in the first page of data
            LoadData(0);
        }

        /// <summary>
        /// Populates the data with a lot of records
        /// </summary>
        private void LoadData(int pageStartIndex)
        {
            // grab the last index of the data to jump to when we are finished
            var previousLastIndex = _data.Count;

            // calculate the last index of the new list
            var lastIndex = _data.Count + pageCount;
            
            // **关键修改**: 插入数据到列表的前面
            if (pageStartIndex == 0)
            {
                for (int i = lastIndex - 1; i >= pageStartIndex; i--)
                {
                    _data.Add(new Data() { someText = "Cell Data Index " + i.ToString() });
                }
            }
            else
            {
                for (int i = pageStartIndex; i <= lastIndex; i++)
                {
                    _data.Insert(new Data() { someText = "Cell Data Index " + i.ToString() }, 0);
                }
            }

            // tell the scroller to reload now that we have the data.
            scroller.ReloadData();

            // **关键修改**: 让Scroller维持原本的可见区域
            scroller.JumpToDataIndex(pageCount + 2, 1f, 1f);

            // toggle off loading new so that we can load again at the bottom of the scroller
            _loadingNew = false;
        }

        #region EnhancedScroller Handlers

        /// <summary>
        /// This tells the scroller the number of cells that should have room allocated. 
        /// This should be the length of your data array plus one for the loading cell.
        /// </summary>
        /// <param name="scroller">The scroller that is requesting the data size</param>
        /// <returns>The number of cells</returns>
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            // in this example, we just pass the number of our data elements
            // plus one for the loading cell
            return _data.Count + 1;
        }

        /// <summary>
        /// This tells the scroller what the size of a given cell will be. Cells can be any size and do not have
        /// to be uniform. For vertical scrollers the cell size will be the height. For horizontal scrollers the
        /// cell size will be the width.
        /// </summary>
        /// <param name="scroller">The scroller requesting the cell size</param>
        /// <param name="dataIndex">The index of the data that the scroller is requesting</param>
        /// <returns>The size of the cell</returns>
        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return cellHeight;
        }

        /// <summary>
        /// Gets the cell to be displayed. You can have numerous cell types, allowing variety in your list.
        /// Some examples of this would be headers, footers, and other grouping cells.
        /// </summary>
        /// <param name="scroller">The scroller requesting the cell</param>
        /// <param name="dataIndex">The index of the data that the scroller is requesting</param>
        /// <param name="cellIndex">The index of the list. This will likely be different from the dataIndex if the scroller is looping</param>
        /// <returns>The cell for the scroller to use</returns>
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            if (dataIndex == 0)
            {
                LoadingCellView loadingCellView = scroller.GetCellView(loadingCellViewPrefab) as LoadingCellView;

                loadingCellView.name = "Loading...";

                return loadingCellView;
            }
            else
            {
                int realIndex = dataIndex - 1;
                realIndex = Mathf.Clamp(realIndex, 0, _data.Count - 1); // **确保索引在 0 ~ _data.Count-1 范围内**
                
                // first, we get a cell from the scroller by passing a prefab.
                // if the scroller finds one it can recycle it will do so, otherwise
                // it will create a new cell.
                CellView cellView = scroller.GetCellView(cellViewPrefab) as CellView;

                // set the name of the game object to the cell's data index.
                // this is optional, but it helps up debug the objects in 
                // the scene hierarchy.
                cellView.name = "Cell " + realIndex.ToString();
                cellView.SetData(_data[realIndex]);

                // return the cell to the scroller
                return cellView;
            }
        }

        /// <summary>
        /// This is called when the scroller fires a scrolled event
        /// </summary>
        /// <param name="scroller">the scroller that fired the event</param>
        /// <param name="val">scroll amount</param>
        /// <param name="scrollPosition">new scroll position</param>
        private void ScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            var pos = 0f;
            if (_data.Count > 0)
            {
                pos = 1.0f / _data.Count;
            }
            if (scroller.NormalizedScrollPosition <= pos && !_loadingNew)
            {
                _loadingNew = true;
                StartCoroutine(FakeDelay());
            }
        }

        /// <summary>
        /// This is a method to fake a real world delay in gathering data.
        /// This should not be used in your application
        /// </summary>
        /// <returns>The delay</returns>
        IEnumerator FakeDelay()
        {
            // wait for one second
            yield return new WaitForSeconds(1f);

            // load the data
            LoadData(_data.Count);
        }

        #endregion
    }
}
