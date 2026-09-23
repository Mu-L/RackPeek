const assert = require("node:assert/strict");
const test = require("node:test");

const { screenshotFilename } = require("./capture");

test("generates a Windows-safe filename for the root URL", () => {
  assert.equal(screenshotFilename("http://localhost:5287"), "localhost_5287.png");
});

test("preserves the route in the screenshot filename", () => {
  assert.equal(
    screenshotFilename("http://localhost:5287/visualise/topology"),
    "localhost_5287_visualise_topology.png"
  );
});

test("omits the default HTTPS port", () => {
  assert.equal(
    screenshotFilename("https://example.com:443/a/b"),
    "example.com_a_b.png"
  );
});

test("removes filesystem-unsafe characters", () => {
  const filename = screenshotFilename("http://[::1]:5287/a?b=c");
  assert.equal(filename, "___1__5287_a.png");
  assert.doesNotMatch(filename, /[\\/:*?"<>|]/);
});
