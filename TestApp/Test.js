var system = require('system');
var web = require('webpage');

var url = system.args[1];

var page = web.create();

page.open(url, function (status) {
    if (status === 'success') {
        console.log('Success ' + url + '\n');
        
        page.render('test.png');

        slimer.exit();
        page.close();
        }
});