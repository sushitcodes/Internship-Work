export function Squares() {
  const n = 9;
  const rows = [];

  for (let i = 0; i < 2 * n - 1; i++) {
    let row = "";

    for (let j = 0; j < 2 * n - 1; j++) {
      const minDistance = Math.min(i, j, 2 * n - 2 - i, 2 * n - 2 - j);

      row += n - minDistance + " ";
    }

    rows.push(<div key={i}>{row}</div>);
  }

  return <>{rows}</>;
}
