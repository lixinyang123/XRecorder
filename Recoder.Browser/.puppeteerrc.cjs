const {join} = require('path');

module.exports = {
	cacheDirectory: join(__dirname, './'),
	experiments: {
		macArmChromiumEnabled: true,
	}
};