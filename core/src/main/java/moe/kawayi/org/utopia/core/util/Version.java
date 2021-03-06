// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
// The Version.java is a part of organization moe-org, under MIT License.
// See https://opensource.org/licenses/MIT for license information.
// Copyright (c) 2021-2022 moe-org All rights reserved.
// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

package moe.kawayi.org.utopia.core.util;

import java.util.Properties;
import java.util.concurrent.atomic.AtomicReference;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;

/**
 * 获取版本号
 * <p>
 * 从JAR的utopia-version.properties中读取Utopia-Version条目
 */
public final class Version {
    /**
     * should not be called
     */
    private Version() {}

    private static final AtomicReference<String> VERSION = new AtomicReference<>(null);

    private static final Logger LOGGER = LogManager.getLogger(Version.class);

    /**
     * 读取版本号的properties文件路径（基于此类所在的jar）
     */
    public static final String PROPERTIES_FILE_PATH = "/utopia-version.properties";

    /**
     * 读取版本号文件所使用的properties key
     */
    public static final String VERSION_PROPERTIES_KEY = "UtopiaVersion";

    /**
     * 无法获取到正确版本号所使用的默认版本号
     */
    public static final String DEFAULT_VERSION = "0.0.0";

    /**
     * 获取版本号
     *
     * @return 获取到的版本号。如果获取失败返回0.0.0
     * @throws java.io.IOException IO错误
     */
    @NotNull
    public static String getUtopiaVersion() throws java.io.IOException {
        if (VERSION.get() == null) {
            Properties properties = new Properties();

            var is = Version.class.getResourceAsStream(PROPERTIES_FILE_PATH);

            if (is == null) {
                VERSION.set(DEFAULT_VERSION);
                LOGGER.warn("could not read utopia version");
                return DEFAULT_VERSION;
            }

            try (is) {

                properties.load(is);

                var ver = properties.getProperty(VERSION_PROPERTIES_KEY, DEFAULT_VERSION);

                VERSION.set(ver);

                return ver;
            }
        } else {
            return VERSION.get();
        }
    }
}
