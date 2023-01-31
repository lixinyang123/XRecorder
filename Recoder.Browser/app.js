const puppeteer = require('puppeteer');

(async () => {
    const browser = await puppeteer.launch({
        headless: false,
        defaultViewport: null
    });

    var page = await browser.newPage();
    await page.goto('chrome://new-tab-page/');
})();