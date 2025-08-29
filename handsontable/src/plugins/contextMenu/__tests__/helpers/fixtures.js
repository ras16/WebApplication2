/**
 * Generates items to be used within contextMenu plugin.
 *
 * @param {number} [itemsCount=200] The number of generated items.
 * @param {Function} mapFunction The function that maps each item.
 * @returns {object[]}
 */
export function generateRandomContextMenuItems(itemsCount = 200, mapFunction = (i, item) => item) {
  return Array.from(new Array(itemsCount)).map((_, i) => {
    return mapFunction(i, {
      name: `Test item ${i + 1}`
    });
  });
}
