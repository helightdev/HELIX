import type { BaseLayoutProps } from 'fumadocs-ui/layouts/shared';
import { appName, gitConfig } from './shared';

export function baseOptions(): BaseLayoutProps {
  return {
    nav: {
      // JSX supported
      title: (
          <div style={{display: 'flex', alignItems: 'center', gap: '6px'}}>
              <img
                  src="/icon.svg"
                  alt="Logo"
                  width={12}
                  height={12}
                  className={"ml-3"}
              />
              <span> </span>
              <span className={"text-xl"}><b>HELIX</b></span>
          </div>
      ),
    },
    githubUrl: `https://github.com/${gitConfig.user}/${gitConfig.repo}`,
  };
}
