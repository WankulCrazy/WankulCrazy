import fs, {promises as fsPromises} from 'fs';

async function main() {
    const input = await fsPromises.readFile('wankul_cards.json', 'utf8');
    const parsed = JSON.parse(input);
    const cards = parsed.cards;

    const txDrops = [];

    const isInTxDrops = (value) => {
        for (const element of txDrops) {
            if (element == value) {
                return true;
            }
        }
        return false;
    }

    for (const card of cards) {
        if (!isInTxDrops(card.rarity.txDrop)){
            txDrops.push(card.rarity.txDrop);
        }
    }


    await fsPromises.writeFile('txDrop_wankul_cards.json', JSON.stringify(txDrops, null, 4));
}

main();