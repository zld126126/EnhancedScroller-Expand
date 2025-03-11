using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;

namespace EnhancedScrollerDemos.PaginationFetchSizes
{
    /// <summary>
    /// This demo shows how you can use the calculated size of the cell view to drive the scroller's cell sizes.
    /// This can be good for cases where you do not know how large each cell will need to be until the contents are
    /// populated. An example of this would be text cells containing unknown information.
    /// </summary>
    public class Controller : MonoBehaviour, IEnhancedScrollerDelegate
    {
        /// <summary>
        /// Internal representation of our data. Note that the scroller will never see
        /// this, so it separates the data from the layout using MVC principles.
        /// </summary>
        private List<Data> _data;

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
        /// The number of elements per page
        /// </summary>
        public int pageCount;

        /// <summary>
        /// Used to determine if the scroller is already loading new data.
        /// If so, then we don't want to call again to avoid an infinite loop.
        /// </summary>
        private bool _loadingNew;

        /// <summary>
        /// This member tells the scroller that we need
        /// the cell views to figure out how much space to use.
        /// This is only set to true on the first pass to reduce
        /// processing required.
        /// </summary>
        private bool _calculateLayout;

        void Start()
        {
            scroller.Delegate = this;
            scroller.scrollerScrolled = ScrollerScrolled;
            
            // initialize the data
            _data = new List<Data>();

            // load in the first page of data
            LoadData(0);
        }

        private string GetTxt(int index)
        {
            var txt = "";
            if (index % 7 == 0)
            {
                txt =
                    " Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam augue enim, scelerisque ac diam nec, efficitur aliquam orci. Vivamus laoreet, libero ut aliquet convallis, dolor elit auctor purus, eget dapibus elit libero at lacus. Aliquam imperdiet sem ultricies ultrices vestibulum. Proin feugiat et dui sit amet ultrices. Quisque porta lacus justo, non ornare nulla eleifend at. Nunc malesuada eget neque sit amet viverra. Donec et lectus ac lorem elementum porttitor. Praesent urna felis, dapibus eu nunc varius, varius tincidunt ante. Vestibulum vitae nulla malesuada, consequat justo eu, dapibus elit. Nulla tristique enim et convallis facilisis.";
            }else if (index % 7 == 1)
            {
                txt =
                    " Nunc convallis, ipsum a porta viverra, tortor velit feugiat est, eget consectetur ex metus vel diam.";
            }else if (index % 7 == 2)
            {
                txt =
                    " Phasellus laoreet vitae lectus sit amet venenatis. Duis scelerisque ultricies tincidunt. Cras ullamcorper lectus sed risus porttitor, id viverra urna venenatis. Maecenas in odio sed mi tempus porta et a justo. Nullam non ullamcorper est. Nam rhoncus nulla quis commodo aliquam. Maecenas pulvinar est sed ex iaculis, eu pretium tellus placerat. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Praesent in ipsum faucibus, fringilla lectus id, congue est. ";
            }else if (index % 7 == 3)
            {
                txt = " Fusce ex lectus.";
            }else if (index % 7 == 4)
            {
                txt = " Fusce mollis elementum sem euismod malesuada. Aenean et convallis turpis. Suspendisse potenti.";
            }else if (index % 7 == 5)
            {
                txt =
                    " Fusce nec sapien orci. Pellentesque mollis ligula vitae interdum imperdiet. Aenean ultricies velit at turpis luctus, nec lacinia ligula malesuada. Nulla facilisi. Donec at nisi lorem. Aenean vestibulum velit velit, sed eleifend dui sodales in. Nunc vulputate, nulla non facilisis hendrerit, neque dolor lacinia orci, et fermentum nunc quam vel purus. Donec gravida massa non ullamcorper consectetur. Sed pellentesque leo ac ornare egestas. ";
            }else if (index % 7 == 6)
            {
                txt =
                    " Curabitur non dignissim turpis, vel viverra elit. Cras in sem rhoncus, gravida velit ut, consectetur erat. Proin ac aliquet nulla. Mauris quis augue nisi. Sed purus magna, mollis sed massa ac, scelerisque lobortis leo. Nullam at facilisis ex. Nullam ut accumsan orci. Integer vitae dictum felis, quis tristique sem. Suspendisse potenti. Curabitur bibendum eleifend eros at porta. Ut malesuada consectetur arcu nec lacinia. ";
            }
            return txt;
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
                    _data.Add(new Data() { cellSize = 0, someText = "Index:" + i.ToString() + GetTxt(i) });
                }
            }
            else
            {
                scroller.ClearAll();
                scroller.ScrollPosition = 0;
                foreach (var item in _data)
                {
                    item.cellSize = 0;
                }

                for (int i = pageStartIndex; i <= lastIndex; i++)
                {
                    _data.Insert(0, new Data() { cellSize = 0, someText = "Index:" + i.ToString() + GetTxt(i) });
                }
            }

            // tell the scroller to reload now that we have the data.
            //scroller.ReloadData();
            ResizeScroller();

            // **关键修改**: 让Scroller维持原本的可见区域
            if (pageStartIndex == 0)
            {
                scroller.JumpToDataIndex(pageCount - 1, 1f, 1f);
            }
            else
            {
                scroller.JumpToDataIndex(pageCount + 1, 1f, 1f);
            }

            // toggle off loading new so that we can load again at the bottom of the scroller
            _loadingNew = false;
        }

        /// <summary>
        /// This function will exand the scroller to accommodate the cells, reload the data to calculate the cell sizes,
        /// reset the scroller's size back, then reload the data once more to display the cells.
        /// </summary>
        private void ResizeScroller()
        {
            // capture the scroller dimensions so that we can reset them when we are done
            var rectTransform = scroller.GetComponent<RectTransform>();
            var size = rectTransform.sizeDelta;

            // set the dimensions to the largest size possible to acommodate all the cells
            rectTransform.sizeDelta = new Vector2(size.x, float.MaxValue);

            // First Pass: reload the scroller so that it can populate the text UI elements in the cell view.
            // The content size fitter will determine how big the cells need to be on subsequent passes.
            _calculateLayout = true;
            scroller.ReloadData();

            // reset the scroller size back to what it was originally
            rectTransform.sizeDelta = size;

            // Second Pass: reload the data once more with the newly set cell view sizes and scroller content size
            _calculateLayout = false;
            scroller.ReloadData();
        }

        #region EnhancedScroller Handlers

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _data.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            // we pull the size of the cell from the model.
            // First pass (frame countdown 2): this size will be zero as set in the LoadData function
            // Second pass (frame countdown 1): this size will be set to the content size fitter in the cell view
            // Third pass (frmae countdown 0): this set value will be pulled here from the scroller
            return _data[dataIndex].cellSize;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            // first, we get a cell from the scroller by passing a prefab.
            // if the scroller finds one it can recycle it will do so, otherwise
            // it will create a new cell.
            CellView cellView = scroller.GetCellView(cellViewPrefab) as CellView;

            // set the name of the game object to the cell's data index.
            // this is optional, but it helps up debug the objects in 
            // the scene hierarchy.
            cellView.name = "Cell " + dataIndex.ToString();
            cellView.SetData(_data[dataIndex], _calculateLayout);

            // return the cell to the scroller
            return cellView;
        }
        
        /// <summary>
        /// This is called when the scroller fires a scrolled event
        /// </summary>
        /// <param name="scroller">the scroller that fired the event</param>
        /// <param name="val">scroll amount</param>
        /// <param name="scrollPosition">new scroll position</param>
        private void ScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            if (_calculateLayout)
            {
                Debug.Log("_calculateLayout");
                return;
            }

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
