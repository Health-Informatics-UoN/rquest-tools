// https://nextra.site/docs/docs-theme/theme-configuration
import { Rabbit, Package, PocketKnife } from "lucide-react";

export default {
  logo: (
    <span
      style={{
        display: "flex",
        color: "#0E7490",
      }}
    >
      <Rabbit style={{ marginRight: "5px" }} />
      +
      <Package style={{ marginRight: "5px" }} />
      +
      <PocketKnife style={{ marginRight: "5px" }} />
      =
      <strong style={{ marginRight: "5px" }}>Hutch Tools</strong>Documentation
    </span>
  ),
  project: {
    link: "https://github.com/Health-Informatics-UoN/hutch-cohort-discovery",
  },
  head: (
    <>
      //<link rel="icon" type="image/svg+xml" href="./rabbit.png" /> I thought I had to use these to import the icons, but apparently not
      //<link rel="icon" type="image/svg+xml" href="./package.png" />
      //<link rel="icon" type="image/svg+xml" href="./pocket-knife.png" />
    </>
  ),
  footer: {
    text: (
      <span>
        ©{new Date().getFullYear()}{" "}
        <a href="https://nottingham.ac.uk" target="_blank">
          University of Nottingham
        </a>
      </span>
    ),
  },
};
