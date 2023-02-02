const {join} = require('path');

module.exports = {
	cacheDirectory: join(__dirname, 'chromium'),
	experiments: {
		macArmChromiumEnabled: true,
	}
};