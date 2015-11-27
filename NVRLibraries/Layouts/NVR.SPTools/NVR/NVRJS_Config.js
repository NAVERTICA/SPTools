//require config
require.config({
    paths: {
        'jquery': 'http://ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery',
		'underscore': "https://cdnjs.cloudflare.com/ajax/libs/underscore.js/1.8.3/underscore",
        'nvr': '/_layouts/15/NVR.SPTools/NaverticaJS/NVRJS_Require',
        'nvr_aggregator': '/_layouts/15/NVR.SPTools/NaverticaJS/NVR_aggregator',
        'nvr_base64_url': '/_layouts/15/NVR.SPTools/NaverticaJS/NVR_base64_url',
        'nvr_codemirror': '/_layouts/15/NVR.SPTools/NaverticaJS/NVR_codemirror',
        'nvr_common': '/_layouts/15/NVR.SPTools/NaverticaJS/NVR_common',
        'nvr_e_n_d_form': '/_layouts/15/NVR.SPTools/NaverticaJS/NVR_e_n_d_form',
        'nvr_envelopes': '/_layouts/15/NVR.SPTools/NaverticaJS/NVR_envelopes',
        'nvr_form': '/_layouts/15/NVR.SPTools/NaverticaJS/NVR_form',
        'nvr_helpdesk': '/_layouts/15/NVR.SPTools/NaverticaJS/NVR_helpdesk',
        'nvr_listView': '/_layouts/15/NVR.SPTools/NaverticaJS/NVR_listView',
        'nvr_notifications': '/_layouts/15/NVR.SPTools/NaverticaJS/NVR_notifications',
        'nvr_ribbon': '/_layouts/15/NVR.SPTools/NaverticaJS/NVR_ribbon',
		'nvr_general':'/_layouts/15/NVR.SPTools/NaverticaJS/NVRJS_general',
    },
    shim: {
        'jQuery': {
            exports: '$'
        },
        'underscore': {
            exports: '_'
        }
    }
});

