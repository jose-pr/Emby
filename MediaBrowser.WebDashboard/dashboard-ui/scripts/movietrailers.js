﻿(function ($, document) {

    var data = {};

    function getQuery() {

        var key = getSavedQueryKey();
        var pageData = data[key];

        if (!pageData) {
            pageData = data[key] = {
                query: {
                    SortBy: "SortName",
                    SortOrder: "Ascending",
                    Recursive: true,
                    Fields: "PrimaryImageAspectRatio,SortName,SyncInfo",
                    ImageTypeLimit: 1,
                    EnableImageTypes: "Primary,Backdrop,Banner,Thumb",
                    StartIndex: 0,
                    Limit: LibraryBrowser.getDefaultPageSize()
                }
            };

            pageData.query.ParentId = LibraryMenu.getTopParentId();
            LibraryBrowser.loadSavedQueryValues(key, pageData.query);
        }
        return pageData.query;
    }

    function getSavedQueryKey() {

        return getWindowUrl();
    }

    function reloadItems(page, viewPanel) {

        Dashboard.showLoadingMsg();

        var query = getQuery();
        query.UserId = Dashboard.getCurrentUserId();

        ApiClient.getJSON(ApiClient.getUrl('Trailers', query)).done(function (result) {

            // Scroll back up so they can see the results from the beginning
            window.scrollTo(0, 0);

            if (result.Items.length) {
                $('.noItemsMessage', page).hide();
            }
            else {
                $('.noItemsMessage', page).show();
            }

            var html = '';
            var pagingHtml = LibraryBrowser.getQueryPagingHtml({
                startIndex: query.StartIndex,
                limit: query.Limit,
                totalRecordCount: result.TotalRecordCount,
                viewButton: true,
                showLimit: false,
                viewPanelClass: 'trailerViewPanel'
            });

            page.querySelector('.listTopPaging').innerHTML = pagingHtml;

            updateFilterControls(page, viewPanel);

            html = LibraryBrowser.getPosterViewHtml({
                items: result.Items,
                shape: "portrait",
                lazy: true,
                showDetailsMenu: true
            });

            var elem = page.querySelector('.itemsContainer');
            elem.innerHTML = html + pagingHtml;
            ImageLoader.lazyChildren(elem);

            $('.btnNextPage', page).on('click', function () {
                query.StartIndex += query.Limit;
                reloadItems(page, viewPanel);
            });

            $('.btnPreviousPage', page).on('click', function () {
                query.StartIndex -= query.Limit;
                reloadItems(page, viewPanel);
            });

            LibraryBrowser.saveQueryValues(getSavedQueryKey(), query);

            Dashboard.getCurrentUser().done(function (user) {

                if (user.Policy.EnableMediaPlayback && result.Items.length) {
                    $('.btnTrailerReel', page).show();
                } else {
                    $('.btnTrailerReel', page).hide();
                }
            });

            Dashboard.hideLoadingMsg();
        });
    }

    function updateFilterControls(tabContent, viewPanel) {

        var query = getQuery();
        // Reset form values using the last used query
        $('.radioSortBy', viewPanel).each(function () {

            this.checked = (query.SortBy || '').toLowerCase() == this.getAttribute('data-sortby').toLowerCase();

        }).checkboxradio('refresh');

        $('.radioSortOrder', viewPanel).each(function () {

            this.checked = (query.SortOrder || '').toLowerCase() == this.getAttribute('data-sortorder').toLowerCase();

        }).checkboxradio('refresh');

        $('.chkStandardFilter', viewPanel).each(function () {

            var filters = "," + (query.Filters || "");
            var filterName = this.getAttribute('data-filter');

            this.checked = filters.indexOf(',' + filterName) != -1;

        }).checkboxradio('refresh');

        $('.alphabetPicker', tabContent).alphaValue(query.NameStartsWithOrGreater);
        $('select.selectPageSize', viewPanel).val(query.Limit).selectmenu('refresh');
    }

    function playReel(page) {

        $('.popupTrailerReel', page).popup('close');

        var reelQuery = {
            UserId: Dashboard.getCurrentUserId(),
            SortBy: 'Random',
            Limit: 50,
            Fields: "MediaSources,Chapters"
        };

        if ($('.chkUnwatchedOnly', page).checked()) {
            reelQuery.Filters = "IsPlayed";
        }

        ApiClient.getJSON(ApiClient.getUrl('Trailers', reelQuery)).done(function (result) {

            MediaController.play({
                items: result.Items
            });
        });
    }

    function onSubmit() {
        var page = $(this).parents('.page');

        playReel(page);
        return false;
    }

    $(document).on('pageinitdepends', "#moviesRecommendedPage", function () {

        var page = this;
        var index = 2;
        var tabContent = page.querySelector('.pageTabContent[data-index=\'' + index + '\']');
        var viewPanel = $('.trailerViewPanel', page);

        $(page.querySelector('neon-animated-pages')).on('tabchange', function () {

            if (parseInt(this.selected) == index) {
                if (LibraryBrowser.needsRefresh(tabContent)) {
                    reloadItems(tabContent, viewPanel);
                    updateFilterControls(tabContent, viewPanel);
                }
            }
        });

        $('.radioSortBy', viewPanel).on('click', function () {
            var query = getQuery();
            query.StartIndex = 0;
            query.SortBy = this.getAttribute('data-sortby');
            reloadItems(tabContent, viewPanel);
        });

        $('.radioSortOrder', viewPanel).on('click', function () {
            var query = getQuery();
            query.StartIndex = 0;
            query.SortOrder = this.getAttribute('data-sortorder');
            reloadItems(tabContent, viewPanel);
        });

        $('.chkStandardFilter', viewPanel).on('change', function () {

            var query = getQuery();
            var filterName = this.getAttribute('data-filter');
            var filters = query.Filters || "";

            filters = (',' + filters).replace(',' + filterName, '').substring(1);

            if (this.checked) {
                filters = filters ? (filters + ',' + filterName) : filterName;
            }

            query.StartIndex = 0;
            query.Filters = filters;

            reloadItems(tabContent, viewPanel);
        });

        $('.alphabetPicker', tabContent).on('alphaselect', function (e, character) {

            var query = getQuery();
            query.NameStartsWithOrGreater = character;
            query.StartIndex = 0;

            reloadItems(tabContent, viewPanel);

        }).on('alphaclear', function (e) {

            var query = getQuery();
            query.NameStartsWithOrGreater = '';

            reloadItems(tabContent, viewPanel);
        });

        $('.itemsContainer', tabContent).on('needsrefresh', function () {

            reloadItems(tabContent, viewPanel);

        });

        $('select.selectPageSize', viewPanel).on('change', function () {
            var query = getQuery();
            query.Limit = parseInt(this.value);
            query.StartIndex = 0;
            reloadItems(tabContent, viewPanel);
        });

        $('.btnTrailerReel', tabContent).on('click', function () {

            $('.popupTrailerReel', page).popup('open');

        });

        $('.popupTrailerReelForm', page).off('submit', onSubmit).on('submit', onSubmit);
    });

})(jQuery, document);