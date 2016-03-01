(function ($, document, window) {
    
    function init(tunerPage) {

        var url = 'components/tunerproviders/' + tunerPage.CurrentInfo.Type + '.js';

        require([url], function (factory) {

            var instance = new factory(tunerPage, {
            });

            instance.init();
        });
    }
    function getTunerName(providerId) {

        providerId = providerId.toLowerCase();

        switch (providerId) {

            case 'm3u':
                return 'M3U Playlist';
            case 'hdhomerun':
                return 'HDHomerun';
            default:
                return 'Unknown';
        }
    }

    function loadTemplate(tunerPage) {

        var xhr = new XMLHttpRequest();
        xhr.open('GET', 'components/tunerproviders/' + tunerPage.CurrentInfo.Type + '.html', true);

        xhr.onload = function (e) {

            var html = this.response;
            var elem = tunerPage.querySelector('.providerTemplate');
            elem.innerHTML = Globalize.translateDocument(html);
            $(elem.parentNode.parentNode).trigger('create');

            init(tunerPage);
        }

        xhr.send();
    }

    function BaseSubmitInfo(tunerPage) {

        Dashboard.showLoadingMsg();
        tunerPage.CurrentInfo.IsEnabled = tunerPage.querySelector('.chkEnabled').checked;
        tunerPage.CurrentInfo.Url = tunerPage.querySelector('.txtDevicePath').value;
        tunerPage.CurrentInfo.DataVersion = 1;
        tunerPage.CurrentInfo.ChannelMaps = tunerPage.querySelector('.txtChannelMaps').value;
        tunerPage.CurrentInfo.ListingsProvider = tunerPage.querySelector('#selectListing').value;

        ApiClient.ajax({
            type: "POST",
            url: ApiClient.getUrl('LiveTv/TunerHosts'),
            data: JSON.stringify(tunerPage.CurrentInfo),
            contentType: "application/json"

        }).then(function () {

            Dashboard.processServerConfigurationUpdateResult();
            Dashboard.navigate('livetvstatus.html');

        }, function () {
            Dashboard.hideLoadingMsg();
            Dashboard.alert({
                message: Globalize.translate('ErrorSavingTvProvider')
            });
        });

    }
    function BaseReload(tunerPage) {
        var wait = $.Deferred();
        tunerPage.querySelector('.txtDevicePath').value = '';
        if (tunerPage.CurrentInfo.Id) {
            ApiClient.getNamedConfiguration("livetv").then(function (config) {
                tunerPage.CurrentInfo = config.TunerHosts.filter(function (i) { return i.Id == tunerPage.CurrentInfo.Id; })[0];
                tunerPage.querySelector('.txtDevicePath').value = tunerPage.CurrentInfo.Url || '';
                tunerPage.querySelector('.txtChannelMaps').value = tunerPage.CurrentInfo.ChannelMaps || '';
                tunerPage.querySelector('.chkEnabled').checked = tunerPage.CurrentInfo.IsEnabled;
                getAllListings(config.ListingProviders, tunerPage).always(function (val) {
                    tunerPage.querySelector('#selectListing').value = tunerPage.CurrentInfo.ListingsProvider || '';
                    wait.resolve(tunerPage.CurrentInfo)
                });
            });
        } else {
            tunerPage.querySelector('.chkEnabled').checked = true;
            wait.resolve(tunerPage.CurrentInfo);
        }
        return wait;
    }
    function getAllListings(listingsProviders, tunerPage) {
        var wait = $.Deferred();
        tunerPage.querySelector('#selectListing').innerHTML = '';
        if (!listingsProviders) { wait.resolve(true); return wait; }
        var waits = [];
        for (var index in listingsProviders) {
            var provider = listingsProviders[index];
            console.log("Getting listings for: " + provider.Id)
            waits.push(refreshListings(provider, tunerPage));
        }
        $.when.apply(null, waits).always(function (val) { wait.resolve(true); });
        return wait;
    }
    function refreshListings(provider, tunerPage) {
        var wait = $.Deferred();

        if (!provider.Id) {
            wait.resolve(true);
            return wait;
        }

        var selector = tunerPage.querySelector('#selectListing');
        console.log(tunerPage);

        ApiClient.ajax({
            type: "GET",
            url: ApiClient.getUrl('LiveTv/ListingProviders/Lineups', {
                Id: provider.Id,
            }),
            dataType: 'json'

        }).then(function (result) {
            var html = selector.innerHTML + result.map(function (o) {
                return '<option value="' + provider.Id + "_" + o.Id + '">' + o.Name + '</option>';
            });
            selector.innerHTML = html;       
            wait.resolve(true);
        }, function (result) {wait.resolve(true); });
        return wait;
    }

    $(document).on('pageshow', "#liveTvTunerProviderPage", function () {
        Dashboard.showLoadingMsg();
        var tunerPage = this;
        tunerPage.CurrentInfo = { Id: getParameterByName('id'), Type: getParameterByName('type') }
        tunerPage.SubmitInfo = function () { BaseSubmitInfo(tunerPage); return false; };
        tunerPage.ReloadInfo = function () { return BaseReload(tunerPage); };
        tunerPage.querySelector('.tunerHeader').innerHTML = getTunerName(tunerPage.CurrentInfo.Type) + " Setup";
        loadTemplate(tunerPage);
    });

})(jQuery, document, window);
