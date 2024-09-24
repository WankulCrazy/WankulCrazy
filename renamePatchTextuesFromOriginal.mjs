import fspromise from 'fs/promises';

function rename(oldPath, newPath) {
	console.log(oldPath);
	console.log(newPath);
	
	return fspromise.rename(oldPath, newPath);
}

function extractAssetNameAndPathID(file) {
	const match = file.match(/-sharedassets1\.assets-\d+/);
	if (match) {
		const assetName = file.split(/-sharedassets1\.assets-\d+/)[0];
		const assetPathID = match[0];
		return { assetName, assetPathID };
	} else {
		return null;
	}
}

async function main() {
	const originalDir = "original";
	const originalFiles = await fspromise.readdir(originalDir);
	const originalAssets = [];
	for (const originalFile of originalFiles) {
		if (originalFile.endsWith('.png')) {
			const originalAsset = extractAssetNameAndPathID(originalFile);
			if (originalAsset) {
				originalAssets.push(originalAsset);
			}
		}
	}


	const patchFiles = await fspromise.readdir("./");
	const patchAssets = [];
	for (const patchFile of patchFiles) {
		if (patchFile.endsWith('.png')) {
			const patchAsset = extractAssetNameAndPathID(patchFile);
			if (patchAsset) {
				patchAssets.push(patchAsset);
			}
		}
	}

	for (const patchAsset of patchAssets) {
		for (const originalAsset of originalAssets) {
			if (originalAsset.assetName === patchAsset.assetName) {
				const oldPath = `./${patchAsset.assetName}${patchAsset.assetPathID}.png`;
				const newPath = `./${patchAsset.assetName}${originalAsset.assetPathID}.png`;
				await rename(oldPath, newPath);
			}
		}
	}
}

main();